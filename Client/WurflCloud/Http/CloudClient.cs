/**
 * Copyright (c) 2015 ScientiaMobile Inc.
 *
 * The WURFL Cloud Client is intended to be used in both open-source and
 * commercial environments. To allow its use in as many situations as possible,
 * the WURFL Cloud Client is dual-licensed. You may choose to use the WURFL
 * Cloud Client under either the GNU GENERAL PUBLIC LICENSE, Version 2.0, or
 * the MIT License.
 *
 * Refer to the COPYING.txt file distributed with this package.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using ScientiaMobile.WurflCloud.Cache;
using ScientiaMobile.WurflCloud.Config;
using ScientiaMobile.WurflCloud.Device;
using ScientiaMobile.WurflCloud.Utils;
using System.Collections.Specialized;

namespace ScientiaMobile.WurflCloud.Http
{
    /// <summary>
    /// Wrapper class used to manage each request to the WURFL cloud.
    /// </summary>
    internal class CloudClient
    {
        // Global constants
        const String ReqPathPrefix = "/v1/json/search:('";
        const String ReqPathSuffix = "')";

        // Private members
        private readonly CloudClientConfig _config;
        private readonly String[] _capabilities;
        private readonly Credentials _credentials;
        private readonly IWurflCloudCache _cache;
        private readonly Dictionary<String, String> _reqHeaders;
        private String _userAgent;

        /// <summary>
        /// Sets up the client 
        /// </summary>
        /// <param name="config">Configuration data for the WURFL client</param>
        /// <param name="capabilities">List of capabilities to retrieve</param>
        /// <param name="credentials">Credentials for the service</param>
        /// <param name="cache">Cache to use</param>
        public CloudClient(CloudClientConfig config, String[] capabilities, Credentials credentials, IWurflCloudCache cache) 
        {
            _reqHeaders = new Dictionary<String, String>();
            _capabilities = capabilities;
            _config = config;
            _credentials = credentials;
            _cache = cache;
        }

        /// <summary>
        /// Prepares the request to be sent to the cloud (adds headers and formats the URL)
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        /// <param name="userAgent">Alternate UA???</param>
        /// <param name="reqParams">Params ???</param>
        /// <returns>URL for the cloud request</returns>
        public String InitializeRequest(IServiceProvider externalContext, String userAgent, Dictionary<String, String> reqParams)
        {
            if (externalContext == null)
            {
                return PrepareRequestFromUserAgentOnly(userAgent, reqParams);
            }

            var context = new GeneralHttpContext(externalContext);

            // If the reportInterval is enabled and past the report age, include the report data in the next request
            //if (_config.ReportInterval > 0 && _cache.GetReportAge() >= _config.ReportInterval)
            //    AddCloudCountersHeader();

            // Prepare the request
            var request = context.Request;
            if (request == null)
                return String.Empty;
            var headers = request.Headers;

            // Grab (and concatenates) server variables for reporting purposes
            var remoteAddress = String.Empty;
            var httpForwarded = String.Empty;
            if (request.ServerVariables != null)
            {
                remoteAddress = request.ServerVariables[HeaderNames.RemoteAddr];
                httpForwarded = request.ServerVariables[HeaderNames.HttpXForwardedFor];
            }
            var via = String.Join(",", new[] { remoteAddress, httpForwarded }).Trim(',').Trim();
            AddRequestHeader(HeaderNames.XForwardedFor, via);

            // Copy input headers into the internal collection
            if (headers != null)
            {
                for (var i = 0; i < headers.Count; i++)
                {
                    var key = headers.GetKey(i);
                    var value = headers.Get(i);
                    AddRequestHeader(key, value);
                }
            }

            // Ensure a user-agent header is present
            EnsureUserAgent(userAgent);

            // Keep on preparing internal data for the cloud request: now adding capabilities
            var reqPath = BuildRequestPath(_capabilities);

            // Finally, adding other headers
            AddOtherHeaders(reqPath);
            return reqPath;
        }

        /// <summary>
        /// Arranges and executes an HTTP request targeted to the WURFL cloud.
        /// The methods first queries the cache, then the cloud. If the cloud is not
        /// available, the method makes a further attempt with the recovery file. 
        ///
        /// Recovery files are NOT currently implemented.
        /// </summary>
        ///<param name="context">Implementation of IServiceProvider Interface</param>
        ///<param name="url">URL for the request</param>
        ///<param name="uaMayBeDifferent">UA to use may be different from what appears in the request</param>
        ///<returns>Device information</returns>
        public DeviceInfo SendRequest(IServiceProvider externalContext, String url, Boolean uaMayBeDifferent)
        {
            if (externalContext == null)
            {
                return SendRequestFromUserAgentOnly(url);
            }

            const String protocol = "http";
            var overriddenUserAgent = String.Empty;

            // Attempt to get information from the cache
            DeviceInfo device;
            if (!uaMayBeDifferent || (_cache is CookieWurflCloudCache))
                device = _cache.GetDevice(externalContext);
            else
            {
                overriddenUserAgent = _userAgent;
                device = _cache.GetDevice(_userAgent);
            }

            if (device.Capabilities.Count > 0)
                return device;
             
            // Information was not found in the cache; let's go with the cloud...

            // Let's complete the string for the URL to invoke
            var host = _config.GetServer().Host;
            var fullyQualifiedUrl = String.Format("{0}://{1}{2}", protocol, host, url);

            // We should be ready for placing the HTTP call ... 
            var webRequest = BuildWebRequestObject(fullyQualifiedUrl, _reqHeaders);
            WebResponse response = null;
            try
            {
                response = webRequest.GetResponse();
            }
            catch (WebException e)
            {
                var exceptionResponse = e.Response as HttpWebResponse;
                var d = new DeviceInfo();
                if (exceptionResponse != null)
                {
                    switch (exceptionResponse.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            d.Errors.Add("Exception", "Invalid API key");
                            break;
                        case HttpStatusCode.PaymentRequired:
                            d.Errors.Add("Exception", "No API key was provided");
                            break;
                        case HttpStatusCode.Forbidden:
                            d.Errors.Add("Exception", "API key is expired or revoked");
                            break;
                        default:
                            d.Errors.Add("The WURFL Cloud service returned an unexpected response", string.Format("{0}: {1}", exceptionResponse.StatusCode, exceptionResponse.StatusDescription));
                            break;
                    }
                }
                else
                {
                    d.Errors.Add("Exception","Timeout");
                }
                return d;
            }
            
            // Capture Web response to a Device object
            device = ProcessWebResponse(externalContext, response, overriddenUserAgent);
            return device;
        }

        #region General Structs for Iservice implementations
        internal class GeneralRequest
        {
            internal NameValueCollection Headers { get; private set; }
            internal NameValueCollection ServerVariables { get; private set; }
            internal GeneralRequest(object request) 
            {
                Headers = request.GetType().GetProperty("Headers").GetValue(request, null) as NameValueCollection;
                ServerVariables = request.GetType().GetProperty("ServerVariables").GetValue(request, null) as NameValueCollection;
            }

        }
        
        internal class GeneralHttpContext
        {
            internal GeneralRequest Request { get; private set; }
            internal GeneralHttpContext(IServiceProvider serviceProvider)
            {
                Request = new GeneralRequest(serviceProvider.GetType().GetProperty("Request").GetValue(serviceProvider, null));
            }
        }
        #endregion


        #region Request Helpers
        /// <summary>
        /// Add a new header (X-Cloud-Counters) to the list of request headers to query for cloud counters.
        /// </summary>
        private void AddCloudCountersHeader()
        {
            // Prepare the value for the X-Cloud-Counters header
            var counters = _cache.GetCounters();
            var builder = new StringBuilder();
            foreach(var counter in counters)
                builder.AppendFormat("{0}:{1},", counter.Key, counter.Value);

            // Gets the string without the trailing comma (if any)
            var headerValue = builder.ToString().Trim(',');
            
            // Appended to the list of headers for 
            AddRequestHeader(HeaderNames.XCloudCounters, headerValue);

            // Reset cache settings
            _cache.ResetCounters();
        }

        /// <summary>
        /// Prepares the request to be sent to the cloud (only using UA) 
        /// </summary>
        /// <param name="userAgent">User agent string being passed</param>
        /// <param name="reqParams">Optional params ???</param>
        /// <returns>URL for the cloud request</returns>
        private String PrepareRequestFromUserAgentOnly(String userAgent, Dictionary<String, String> reqParams)
        {
            EnsureUserAgent(userAgent); 

            // Keep on preparing internal data for the cloud request: now adding capabilities
            var reqPath = BuildRequestPath(_capabilities);

            // Finally, adding other headers
            AddOtherHeaders(reqPath);
            return reqPath;
        }

        /// <summary>
        /// Arranges and executes an HTTP request targeted to the WURFL cloud.
        /// The methods first queries the cache, then the cloud. If the cloud is not
        /// available, the method makes a further attempt with the recovery file. 
        ///
        /// Recovery files are NOT currently implemented.
        /// </summary>
        ///<param name="url">URL for the request</param>
        ///<returns>Device information</returns>
        private DeviceInfo SendRequestFromUserAgentOnly(String url)
        {
            const String protocol = "http";
            var overriddenUserAgent = _userAgent;

            // Attempt to get information from the cache
            var device = _cache.GetDevice(_userAgent);
            if (device.Capabilities.Count > 0)
                return device;

            // Information was not found in the cache; let's go with the cloud...

            // Let's complete the string for the URL to invoke
            var host = _config.GetServer().Host;
            var fullyQualifiedUrl = String.Format("{0}://{1}{2}", protocol, host, url);

            // We should be ready for placing the HTTP call ... 
            var webRequest = BuildWebRequestObject(fullyQualifiedUrl, _reqHeaders);
            WebResponse response = null;

            try
            {
                response = webRequest.GetResponse();
            }
            catch (WebException e)
            {
                var exceptionResponse = e.Response as HttpWebResponse;
                var d = new DeviceInfo();
                if (exceptionResponse != null)
                {
                    switch (exceptionResponse.StatusCode)
                    {
                        case HttpStatusCode.Unauthorized:
                            d.Errors.Add("Exception", "Invalid API key");
                            break;
                        case HttpStatusCode.PaymentRequired:
                            d.Errors.Add("Exception", "No API key was provided");
                            break;
                        case HttpStatusCode.Forbidden:
                            d.Errors.Add("Exception", "API key is expired or revoked");
                            break;
                        default:
                            d.Errors.Add("The WURFL Cloud service returned an unexpected response", string.Format("{0}: {1}", exceptionResponse.StatusCode, exceptionResponse.StatusDescription));
                            break;
                    }
                }
                else
                {
                    d.Errors.Add("Exception", "Timeout");
                }
                return d;
            }

            // Capture Web response to a Device object
            device = ProcessWebResponse(null, response, overriddenUserAgent);
            return device;
        }

        /// <summary>
        /// Add a new header to the internal collection of request headers to be uploaded 
        /// in the request to the cloud.
        /// </summary>
        /// <param name="headerName">Header name</param>
        /// <param name="headerValue">Header value</param>
        private void AddRequestHeader(String headerName, String headerValue)
        {
             _reqHeaders[headerName] = headerValue;
        }

        /// <summary>
        /// Ensures the internal collection of headers includes the most important 
        /// of all HTTP headers (for our purposes): the User-Agent header.
        /// </summary>
        private void EnsureUserAgent(String userAgent /* not used */)
        {
            string ua = null;
            if (userAgent != null)
                ua = userAgent.Trim();

            _userAgent = _reqHeaders.ContainsKey(HeaderNames.UserAgent)
                             ? _reqHeaders[HeaderNames.UserAgent]
                             : String.Empty;
            if (!String.IsNullOrEmpty(ua))
            {
                _userAgent = ua;
                if (!_reqHeaders.ContainsKey(HeaderNames.UserAgent))
                {
                    _reqHeaders[HeaderNames.UserAgent] = ua;
                }                      
            }
        }

        /// <summary>
        /// Builds the URL for the cloud request.
        /// </summary>
        /// <param name="capabilities">List of non-empty/null capabilities</param>
        /// <returns>URL as a string</returns>
        private static String BuildRequestPath(String[] capabilities)
        {
            var capabilitiesAsText = String.Join(",", capabilities);
            return String.Format("{0}{1}{2}", ReqPathPrefix, capabilitiesAsText, ReqPathSuffix);
        }

        /// <summary>
        /// Completes the cloud request with encoding, connection, authorization and other custom headers.
        /// </summary>
        /// <param name="reqPath">URL of the cloud server</param>
        private void AddOtherHeaders(String reqPath)
        {
            SetEncodingAccept();
            AddRequestHeader(HeaderNames.Accept, "*/*");
            AddRequestHeader(HeaderNames.XCloudClient, GetCloudClientSignature());
            AddRequestHeader(HeaderNames.Connection, "Close");
            AddRequestHeader(HeaderNames.Authorization, GetBasicAuthString(reqPath, _userAgent));
        }

        /// <summary>
        /// Appends the Accept-Encoding header.
        /// </summary>
        private void SetEncodingAccept()
        {
            if (_config.Compression)
                AddRequestHeader(HeaderNames.XAcceptEncoding, Encodings.Gzip.ToString());
        }

        /// <summary>
        /// Returns the BASIC authorization string for the request
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="userAgent">User agent</param>
        /// <returns>String</returns>
        private String GetBasicAuthString(String path, String userAgent)
        {
            try
            {
                return String.Format("Basic {0}", GetEncodedSignature(path, userAgent).Trim());
            }
            catch (Exception)
            {
                throw new ArgumentException("Signature not valid");
            }
        }

        /// <summary>
        /// Returns the complete signature (including version) of the current version of the client library.
        /// </summary>
        /// <returns>Version as a string</returns>
        private static String GetCloudClientSignature()
        {
            return "WurflCloudClient/DotNet_" + Constants.ClientVersion;
        }

        /// <summary>
        /// Returns the signature
        /// </summary>
        /// <param name="path">Request path</param>
        /// <param name="userAgent">User agent</param>
        /// <returns>String</returns>
        private String GetEncodedSignature(String path, String userAgent)
        {
            var text = String.Format("{0}:{1}", _credentials.UserName, _credentials.Password); 
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(text)); 
        }
        private String GetHashedSignature(String path, String userAgent)
        {
            var text = String.Format("{0}{1}{2}", path, userAgent, _credentials.Password);
            return HashHelper.GetHash(text);
        }
        #endregion

        #region HTTP Helpers

        /// <summary>
        /// Prepares a HttpWebRequest object to carry out the HTTP request.
        /// </summary>
        /// <param name="url">URL to invoke</param>
        /// <param name="headers">HTTP headers to send</param>
        /// <returns></returns>
        private HttpWebRequest BuildWebRequestObject(String url, Dictionary<String, String> headers)
        {
            var webRequest  = WebRequest.Create(url) as HttpWebRequest;
            if (webRequest == null)
                throw new InvalidCastException("Not a HttpWebRequest object");
            
            webRequest.Proxy = _config.Proxy;

            webRequest.Timeout = _config.ConnectionTimeout;
            webRequest.ReadWriteTimeout = _config.ReadTimeout;

            // Copy given headers into the request object
            foreach(var header in headers)
            {
                if (header.Key == HeaderNames.Connection)
                {
                    webRequest.KeepAlive = (header.Value.ToLower() != "close");
                    continue;
                }
                if (header.Key == HeaderNames.UserAgent)
                {
                    webRequest.UserAgent = header.Value;
                    continue;
                }
                if (header.Key == HeaderNames.Accept)
                {
                    webRequest.Accept = header.Value;
                    continue;
                }
                
                try
                {
                    webRequest.Headers.Add(header.Key, header.Value);
                }
                catch
                {
                }
            }

            return webRequest;
        }

        /// <summary>
        /// Capture Web response to a string
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        /// <param name="response">WebResponse as received from .NET</param>
        /// <param name="overriddenUserAgent">The UA to use for caching if specified</param>
        /// <returns>Content as an abstract device or null</returns>
        private DeviceInfo ProcessWebResponse(IServiceProvider context, WebResponse response, String overriddenUserAgent)
        {
            // Grab the response as raw text
            var responseAsText = "";
            var stream = response.GetResponseStream();
            if (stream == null)
                return null;

            // Check whether response is gzip'd
            var enc = response.Headers[HeaderNames.ContentEncoding];
            if (enc != null)
            {
                // By design, it can only be GZIP
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    var reader = new StreamReader(gzip);
                    responseAsText = reader.ReadToEnd();
                    reader.Close();
                }
            }
            else
            {
                // Read as clear text
                var reader = new StreamReader(stream);
                responseAsText = reader.ReadToEnd();
                reader.Close();                
            }

            // Map manually to CloudResponse
            var cloudResponse = MapJsonToResponse(responseAsText);
            var info = new DeviceInfo(cloudResponse);  

            // Cache device and return
            var cacheableDeviceInfo = new DeviceInfo(info, ResponseType.Cache);
            if (!String.IsNullOrEmpty(overriddenUserAgent))
                _cache.SetDevice(overriddenUserAgent, cacheableDeviceInfo);
            else
                _cache.SetDevice(context, cacheableDeviceInfo);
            return info;
        }

        /// <summary>
        /// Maps JSON text to a CloudResponse object
        /// </summary>
        /// <param name="jsonAsText">JSON to parse</param>
        /// <returns>CloudResponse object or null</returns>
        private static CloudResponse MapJsonToResponse(String jsonAsText)
        {
            if (jsonAsText == null) 
                throw new ArgumentNullException("jsonAsText");

           // Map JSON to a dictionary
            //var serializer = new JavaScriptSerializer();
            var json = new Dictionary<String, Object>();
            try
            {
                json = JsonConvert.DeserializeObject<Dictionary<String, Object>>(jsonAsText);
            }
            catch
            {
                throw new HttpException(500, "Unable to parse JSON response from server.");
            }

            // Fill up the CloudResponse object
            var cloudResponse = new CloudResponse
                                    {
                                        ApiVersion = json["apiVersion"].ToString(),
                                        Id = json["id"].ToString()
                                    };
            long d = 0;
            var result = long.TryParse(json["mtime"].ToString(), out d);
            if (result)
                cloudResponse.MTime = d;

            // Add errors
            try
            {
                var errors = (Newtonsoft.Json.Linq.JObject)json["errors"];
                if (errors != null)
                {
                    foreach (var e in errors)
                    {
                        cloudResponse.Errors.Add(e.Key, e.Value.ToString());
                    }
                }
            }
            catch
            {
                cloudResponse.Errors.Add("Exception", "Unable to retrieve errors.");
            }

            // Add capabilities
            try
            {
                var caps = (Newtonsoft.Json.Linq.JObject)json["capabilities"];
                if (caps != null)
                {
                    foreach (var c in caps)
                    {
                        cloudResponse.Capabilities.Add(c.Key, c.Value.ToString());
                        string value;
                        if (!cloudResponse.Capabilities.TryGetValue(c.Key, out value))
                        {
                            cloudResponse.Errors.Add("Exception", String.Format("The {0} capability is not currently available", c.Key));
                        }
                    }
                }
            }
            catch
            {
                cloudResponse.Errors.Add("Exception", "Unable to retrieve capabilities.");
            }

            return cloudResponse;
        }
        #endregion
    }
}