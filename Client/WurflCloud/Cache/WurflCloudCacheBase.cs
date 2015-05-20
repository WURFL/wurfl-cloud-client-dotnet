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
using ScientiaMobile.WurflCloud.Device;

namespace ScientiaMobile.WurflCloud.Cache
{
    /// <summary>
    /// Defines basic functionality of the WURFL cache.
    /// </summary>
    public class WurflCloudCacheBase : IWurflCloudCache 
    {
        // Counters
        protected static String Hit = "hit";
        protected static String Miss = "miss";
        protected static String Error = "error";
        protected static String Age = "age";
        protected Dictionary<String, long> Counters; 

        private long _mtime;
        private long _reportAge;

        protected WurflCloudCacheBase()
        {
            // Prepare counters
            Counters = CreateStatCache();
        }

        #region IWurflCloudCache
        public virtual DeviceInfo GetDevice(IServiceProvider context)
        {
            return new DeviceInfo();
        }
        public virtual DeviceInfo GetDevice(String userAgent)
        {
            return new DeviceInfo();
        }

        public virtual Boolean SetDevice(IServiceProvider context, DeviceInfo device)
        {
            return true;
        }
        public virtual Boolean SetDevice(String userAgent, DeviceInfo device)
        {
            return true;
        }

        public virtual Boolean Purge()
        {
            return true;
        }

        public virtual void Close()
        {
        }

        public virtual void ResetCounters()
        {
            ResetDictionary(Counters);
        }

        public Dictionary<String, long> GetCounters()
        {
            Counters[Age] = GetReportAge();
            return Counters;
        }

        public long GetMtime()
        {
            return _mtime;
        }

        public Boolean SetMtime(long mtime)
        {
            _mtime = mtime;
            return true;
        }

        public long GetReportAge()
        {
            return (DateTime.Now.Millisecond - _reportAge) / 1000;
        }

        public void ResetReportAge()
        {
            _reportAge = DateTime.Now.Millisecond;
        }

        #endregion


        #region Helpers
        protected void IncrementHit()
        {
            var i = Counters[Hit]; 
            Counters[Hit] = i + 1;
        }

        protected void IncrementMiss()
        {
            var i = Counters[Miss];
            Counters[Miss] = i + 1;
        }

        protected void IncrementError()
        {
            var i = Counters[Error];
            Counters[Error] = i + 1;
        }

        protected Dictionary<String, long> CreateStatCache()
        {
            var map = new Dictionary<String, long>();
            ResetDictionary(map);
            return map;
        }

        private static void ResetDictionary(Dictionary<String, long> map)
        {
            map[Hit] = 0L;
            map[Miss] = 0L;
            map[Error] = 0L;
            map[Age] = 0L;
        }
        #endregion
    }
}