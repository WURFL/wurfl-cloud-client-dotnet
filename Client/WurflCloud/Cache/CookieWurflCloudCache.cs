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
using System.Globalization;
using System.Web;
using Newtonsoft.Json;
using ScientiaMobile.WurflCloud.Device;
using ScientiaMobile.WurflCloud.Http;
using System.Reflection;

namespace ScientiaMobile.WurflCloud.Cache
{
    /// <summary>
    /// Implements a WURFL cache using the MemoryCache class of .NET.
    /// </summary>
    public class CookieWurflCloudCache : WurflCloudCacheBase
    {
        private const String CookieName = "WurflCloud_Client";
        private const String CookieEntry = "Data";
        private const Int32 CookieExpiration = 86400;   // seconds (1 day)
        private Boolean _cookieSent;

        #region Overrides
        /// <summary>
        /// Returns known capabilities about the requesting device.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        /// <returns>Information about the device</returns>
        public override DeviceInfo GetDevice(IServiceProvider externalContext)
        {
            if (externalContext == null)
                return new DeviceInfo();
            var context = new GeneralHttpContext(externalContext);
            var cookie = context.Request.Cookies[CookieName];
            if (cookie == null)
                return new DeviceInfo();

            var data = cookie[CookieEntry];
            if (String.IsNullOrEmpty(data))
                return new DeviceInfo();

            var json = context.Server.GeneralUrlDecode("UrlDecode", data);
            if (json == null)
                return new DeviceInfo();
            var content = JsonConvert.DeserializeObject<CookieContent>(json);
            
            if (content == null)
                return new DeviceInfo();

            if (content.DateOfCreation.AddSeconds(CookieExpiration) < DateTime.Now)
                return new DeviceInfo();

            var info = new DeviceInfo(ResponseType.Cache)
                           {
                               ServerVersion = content.ServerVersion,
                               WurflLastUpdate = DateTime.Parse(content.ServerTimestamp, CultureInfo.InvariantCulture),
                               Id = content.DeviceId
                           };

            foreach(var c in content.Capabilities)
                info.Capabilities.Add(c.Key, c.Value);
            return info;      
        }

        public override Boolean SetDevice(IServiceProvider externalContext, DeviceInfo device)
        {
            if (_cookieSent)
                return true; 
            if (device == null)
                return false;
            if (externalContext == null)
                return false;

            var context = new GeneralHttpContext(externalContext);
            var cookie = new HttpCookie(CookieName) { Expires = DateTime.Now.AddSeconds(CookieExpiration) };
            var content = new CookieContent
                              {
                                  Capabilities = device.Capabilities,
                                  DateOfCreation = DateTime.Now,
                                  DeviceId = device.Id,
                                  ServerTimestamp = device.WurflLastUpdate.ToString(CultureInfo.InvariantCulture),
                                  ServerVersion = device.ServerVersion
                              };
            var json = JsonConvert.SerializeObject(content);
            cookie[CookieEntry] = context.Server.GeneralUrlEncode("UrlEncode", json); 
            context.Response.Cookies.Add(cookie);

            _cookieSent = true;
            return true;
        }
        #endregion

        #region General Structs for Iservice implementations
        internal class GeneralResponse
        {
            internal HttpCookieCollection Cookies { get; private set; }
            internal GeneralResponse(object response)
            {
                Cookies = response.GetType().GetProperty("Cookies").GetValue(response, null) as HttpCookieCollection;
            }
        }

        internal class GeneralServer
        {
            internal object Server{get; private set;}
            internal GeneralServer(object server)
            {
                Server = server;
            }
            public string GeneralUrlEncode(string methodName, string parameter) 
            {
                MethodInfo encodeMethod = Server.GetType().GetMethod(methodName, new [] {typeof(string)});
                return (string)encodeMethod.Invoke(Server, new object[] { parameter });
            }
            public string GeneralUrlDecode(string methodName, object parameter)
            {
                MethodInfo decodeMethod = Server.GetType().GetMethod(methodName, new[] { typeof(string) });
                return (string)decodeMethod.Invoke(Server, new object[] { parameter });
            }
        }

        internal class GeneralRequest
        {
            internal HttpCookieCollection Cookies { get; private set; }
            internal GeneralRequest(object request)
            {
                Cookies = request.GetType().GetProperty("Cookies").GetValue(request, null) as HttpCookieCollection;
            }
        }

        internal class GeneralHttpContext
        {
            internal GeneralResponse Response { get; private set; }
            internal GeneralServer Server { get; private set; }
            internal GeneralRequest Request { get; private set; }
            internal GeneralHttpContext(IServiceProvider serviceProvider)
            {
                Response = new GeneralResponse(serviceProvider.GetType().GetProperty("Response").GetValue(serviceProvider, null));
                Server = new GeneralServer(serviceProvider.GetType().GetProperty("Server").GetValue(serviceProvider, null));
                Request = new GeneralRequest(serviceProvider.GetType().GetProperty("Request").GetValue(serviceProvider, null));
            }
        }
        #endregion
    }
}
