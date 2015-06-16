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
using System.Web;
using ScientiaMobile.WurflCloud.Device;
using ScientiaMobile.WurflCloud.Utils;
using ScientiaMobile.WurflCloud.Exc;

namespace ScientiaMobile.WurflCloud.Cache
{
    /// <summary>
    /// Implements a WURFL cache using the MemoryCache class of .NET.
    /// </summary>
    public class MemoryWurflCloudCache : WurflCloudCacheBase
    {
        private readonly System.Web.Caching.Cache _cache;
        private readonly Int32 _durationInSeconds;   // default is 20 mins

        public MemoryWurflCloudCache(Int32 durationInSeconds = 1200)
        {
            _durationInSeconds = durationInSeconds;
            _cache = HttpContext.Current.Cache;  
        }

        #region Overrides
        public override DeviceInfo GetDevice(IServiceProvider externalContext)
        {
            if (externalContext == null)
                return new DeviceInfo();

            var context = new GeneralHttpContext(externalContext);
            // User agent strings are hashed to produce shorter keys for the cache.
            var userAgent = context.Request.UserAgent ?? String.Empty;

            // Attempts to read from the cache
            var device = ReadFromCache(userAgent);
            if (device == null)
            {
                IncrementMiss();
                return new DeviceInfo();
            }

            IncrementHit();
            return device;
        }

        public override DeviceInfo GetDevice(String userAgent)
        {
            if (String.IsNullOrEmpty(userAgent))
                return new DeviceInfo();

            // Attempts to read from the cache
            var device = ReadFromCache(userAgent);
            if (device == null)
            {
                IncrementMiss();
                return new DeviceInfo();
            }

            IncrementHit();
            return device;
        }

        public override Boolean SetDevice(IServiceProvider externalContext, DeviceInfo device)
        {
            if (device == null)
                return false;
            if (externalContext == null)
                return false;
            var context = new GeneralHttpContext(externalContext);

            // User agent strings are hashed to produce shorter keys for the cache.
            var userAgent = context.Request.UserAgent ?? String.Empty;
            WriteToCache(userAgent, device);
            return true;
        }

        public override Boolean SetDevice(String userAgent, DeviceInfo device)
        {
            if (device == null)
                return false;
            if (String.IsNullOrEmpty(userAgent))
                return false;

            // User agent strings are hashed to produce shorter keys for the cache.
            WriteToCache(userAgent, device);
            return true;
        }
        
        public override Boolean Purge()
        {
            ResetCounters();
            return true;
        }

        public override void Close()
        {
            ResetCounters();
        }
        #endregion

        #region General Structs for Iservice implementations
        internal class GeneralRequest
        {
            internal String UserAgent { get; private set; }
            internal GeneralRequest(object request)
            {
                UserAgent = request.GetType().GetProperty("UserAgent").GetValue(request, null) as String;
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

        #region Helpers
        private static String HashUserAgentString(String userAgent)
        {
            return HashHelper.GetHash(userAgent);
        }
        
        private DeviceInfo ReadFromCache(string userAgent)
        {
            var uaHash = HashUserAgentString(userAgent);
            var device = _cache[uaHash] as DeviceInfo;
            return device;
        }

        private void WriteToCache(String userAgent, DeviceInfo device)
        {
            var uaHash = HashUserAgentString(userAgent);
            _cache.Insert(uaHash, device, 
                null, 
                System.Web.Caching.Cache.NoAbsoluteExpiration, 
                TimeSpan.FromSeconds(_durationInSeconds));
        }
        #endregion
    }
}