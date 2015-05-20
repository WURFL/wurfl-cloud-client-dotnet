using System;
using System.Web;
using System.Web.Caching;
using ScientiaMobile.WurflCloud.Cache;
using ScientiaMobile.WurflCloud.Device;
using ScientiaMobile.WurflCloud.Utils;

namespace ScientiaMobile.WurflCloud.Extensions
{
    /// <summary>
    /// Implements a WURFL cache using the MemoryCache class of .NET.
    /// </summary>
    public class FlexibleMemoryWurflCloudCache : WurflCloudCacheBase
    {
        private const String PurgeKey = "Wurfl_Cloud_Purge_Key";
        private readonly System.Web.Caching.Cache _cache;
        private readonly Int32 _durationInSeconds;   // default is 20 mins

        public FlexibleMemoryWurflCloudCache(Int32 durationInSeconds = 1200)
        {
            _durationInSeconds = durationInSeconds;
            _cache = HttpContext.Current.Cache;
            _cache[PurgeKey] = DateTime.Now;
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
            return device ?? new DeviceInfo();
        }

        public override DeviceInfo GetDevice(String userAgent)
        {
            if (String.IsNullOrEmpty(userAgent))
                return new DeviceInfo();

            // Attempts to read from the cache
            var device = ReadFromCache(userAgent);
            return device ?? new DeviceInfo();
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
            _cache[PurgeKey] = DateTime.Now;
            return true;
        }

        public override void Close()
        {
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
            return _cache[uaHash] as DeviceInfo;
        }

        private void WriteToCache(String userAgent, DeviceInfo device)
        {
            var uaHash = HashUserAgentString(userAgent);
            _cache.Insert(uaHash, device,
                new CacheDependency(null, new[] { PurgeKey }), 
                System.Web.Caching.Cache.NoAbsoluteExpiration, 
                TimeSpan.FromSeconds(_durationInSeconds));
        }
        #endregion
    }
}