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
using System.Web;
using ScientiaMobile.WurflCloud.Cache;
using ScientiaMobile.WurflCloud.Config;
using ScientiaMobile.WurflCloud.Device;
using ScientiaMobile.WurflCloud.Http;
using ScientiaMobile.WurflCloud.Request;
using ScientiaMobile.WurflCloud.Utils;

namespace ScientiaMobile.WurflCloud
{
    /// <summary>
    /// Entry point in the WURFL cloud client library. It manages the requests from clients
    /// and relays them to the cloud infrastructure. 
    /// </summary>
    public class CloudClientManager
    {
        private readonly CloudClientConfig _config;
        private Credentials _credentials;
        private IWurflCloudCache _cache;
        private static String _serverVersion = Constants.ServerVersion;
        
        /// <summary>
        /// Creates an instance of the cloud manager class.
        /// </summary>
        public CloudClientManager() : this(new DefaultCloudClientConfig())
        {
        }
        public CloudClientManager(CloudClientConfig config)
        {
            // Sets the internal configuration manager and pick up cloud host to use
            _config = config;

            // Sets the internal cache to be used to serve requests more quickly.
            _cache = new MemoryWurflCloudCache();
            // Throw if invalid credentials are provided.
            EnsureCredentialsAreValid();
        }

        public CloudClientManager(CloudClientConfig config, IWurflCloudCache cache)
        {
            // Sets the internal configuration manager and pick up cloud host to use
            _config = config;

            // Sets the internal cache to be used to serve requests more quickly.
            _cache = cache;

            // Throw if invalid credentials are provided.
            EnsureCredentialsAreValid();
        }

        #region Cloud-related API

        /// <summary>
        /// Returns device information (all available capabilities) for the requesting device.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        /// <param name="capabilities">Array of capabilities</param>
        /// <returns>Device information</returns>
        public DeviceInfo GetDeviceInfo(IServiceProvider context, String[] capabilities)
        {
            var info = DetectDevice(context, String.Empty, null, capabilities, false /* User agent is not different from request */);
            _serverVersion = info.ServerVersion;
            return info;
        }
        public DeviceInfo GetDeviceInfo(String userAgent, String[] capabilities)
        {
            var info = DetectDevice(userAgent, null, capabilities);
            _serverVersion = info.ServerVersion;
            return info;
        }

        /// <summary>
        /// Returns device information for the requesting device.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        /// <returns>Device information</returns>
        public DeviceInfo GetDeviceInfo(IServiceProvider context)
        {
            return GetDeviceInfo(context, new String[] {});
        }

        /// <summary>
        /// Returns given capabilities for the requesting device or specified user-agent/headers.
        /// </summary>
        /// <param name="request">WURFL cloud request object</param>
        /// <param name="capabilities">Capabilities to return</param>
        /// <returns></returns>
        public DeviceInfo GetDeviceInfo(WurflCloudRequest request, String[] capabilities)
        {
            return DetectDevice(request.HttpContext, request.UserAgent, null, capabilities, true /* User agent may be different from request */); 
        }

        /// <summary>
        /// Returns default device information for the requesting device or specified user-agent/headers.
        /// </summary>
        /// <param name="request">WURFL cloud request object</param>
        /// <returns></returns>
        public DeviceInfo GetDeviceInfo(WurflCloudRequest request)
        {
            return GetDeviceInfo(request, new String[] { }); 
        }
        
        /// <summary>
        /// Sets the cache object to use
        /// </summary>
        /// <param name="cache">Cache object to use</param>
        public CloudClientManager SetCache(IWurflCloudCache cache)
        {
            _cache = cache;
            return this;
        }

        #endregion

        #region Informative API
        /// <summary>
        /// Returns the current version of this library.
        /// </summary>
        /// <returns>Version as a string</returns>
        public String GetClientVersion()
        {
            return Constants.ClientVersion;
        }

        /// <summary>
        /// Returns the current version of the WURFL Cloud Server. This information is 
        /// only available after a query has been made. (This information is embedded in the server response.)
        /// </summary>
        /// <returns>Version as a string</returns>
        public String GetApiVersion()
        {
            return _serverVersion;
        }

        /// <summary>
        /// Gets the name of the module that is currently providing caching services.
        /// </summary>
        /// <returns></returns>
        public String GetCachingModuleName()
        {
            return _cache.ToString();
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Lower-level method, returns device information (specified capabilities) given parameters/headers
        /// </summary>
        /// <param name="context"></param>
        /// <param name="userAgent"></param>
        /// <param name="reqParams">Params???</param>
        /// <param name="capabilities">Array of capabilities</param>
        /// <param name="uaMayBeDifferent">UA in request may be different from what appears in Request</param>
        /// <returns>Device information</returns>
        private DeviceInfo DetectDevice(IServiceProvider context, String userAgent, Dictionary<String, String> reqParams, String[] capabilities, Boolean uaMayBeDifferent)
        {
            // Should check whether capabilities include empty/null strings...
            var listOfValidCapabilities = new List<String>(capabilities);
            foreach (var c in capabilities)
            {
                if (String.IsNullOrEmpty(c))
                    listOfValidCapabilities.Remove(c);
            }

            // Relay the request to the CloudClient which will handle cache/cloud request
            var cc = new CloudClient(_config, listOfValidCapabilities.ToArray(), _credentials, _cache);  
            var url = cc.InitializeRequest(context, userAgent, reqParams);
            return cc.SendRequest(context, url, uaMayBeDifferent);
        }
        private DeviceInfo DetectDevice(String userAgent, Dictionary<String, String> reqParams, String[] capabilities)
        {
            const bool uaMayBeDifferent = true;

            // Should check whether capabilities include empty/null strings...
            var listOfValidCapabilities = new List<String>(capabilities);
            foreach (var c in capabilities)
            {
                if (String.IsNullOrEmpty(c))
                    listOfValidCapabilities.Remove(c);
            }

            // Relay the request to the CloudClient which will handle cache/cloud request
            var cc = new CloudClient(_config, listOfValidCapabilities.ToArray(), _credentials, _cache);
            var url = cc.InitializeRequest(null, userAgent, reqParams);
            return cc.SendRequest(null, url, uaMayBeDifferent);
        }

        /// <summary>
        /// Looks into the provided security key and ensure everything is OK
        /// </summary>
        /// <returns>False if security key is invalid or can't be verified.</returns>
        private void EnsureCredentialsAreValid()
        {
            var key = _config.ApiKey;
            if (String.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Api key must be not empty");
            }
            var indexOfColon = key.IndexOf(":");
            if (indexOfColon < 0)
            {
                throw new ArgumentException("Api key must contain a \':\' separator.");
            }
            var user = key.Substring(0, indexOfColon);
            if (user.Length == 0)
            {
                throw new ArgumentException("Api key username is empty.");
            }
            var password = key.Substring(indexOfColon + 1);
            if (password.Length == 0)
            {
                throw new ArgumentException("Api key password is empty.");
            }
            _credentials = new Credentials(user, password);
        }
        #endregion
    }
}