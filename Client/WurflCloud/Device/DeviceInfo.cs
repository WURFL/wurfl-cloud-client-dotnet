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
using ScientiaMobile.WurflCloud.Http;

namespace ScientiaMobile.WurflCloud.Device
{
    /// <summary>
    /// Device as recognized by WURFL.
    /// </summary>
    public class DeviceInfo
    {
        private static DateTime _baseDate = new DateTime(1970, 1, 1);

        public DeviceInfo() 
        {
            Capabilities = new Dictionary<String, String>();
            Errors = new Dictionary<String, String>();
        }

        internal DeviceInfo(DeviceInfo info, ResponseType type) : this(type)
        {
            ServerVersion = info.ServerVersion;
            WurflLastUpdate = info.WurflLastUpdate;
            Id = info.Id;
            foreach (var c in info.Capabilities)
            {
                Capabilities.Add(c.Key, c.Value);
                string value;
                if (!Capabilities.TryGetValue(c.Key, out value))
                    Errors.Add("Exception", String.Format("The {0} capability is not currently available", c.Key));
            }
            foreach (var e in info.Errors)
                Errors.Add(e.Key, e.Value);
        }

        internal DeviceInfo(ResponseType type)
        {
            ResponseOrigin = type;
            Capabilities = new Dictionary<String, String>();
            Errors = new Dictionary<String, String>();
        }

        internal DeviceInfo(CloudResponse response)
            : this(ResponseType.Cloud)
        {
            ServerVersion = response.ApiVersion;
            WurflLastUpdate = _baseDate.AddSeconds(response.MTime);
            Id = response.Id;
            foreach(var c in response.Capabilities)
            {
                Capabilities.Add(c.Key, c.Value);
                string value;
                if (!Capabilities.TryGetValue(c.Key, out value))
                    Errors.Add("Exception", String.Format("The {0} capability is not currently available", c.Key));
            }
            foreach (var e in response.Errors)
                Errors.Add(e.Key, e.Value);
        }

        /// <summary>
        /// List of capabilities that have been retrieved for this device: name/value
        /// </summary>
        public IDictionary<String, String> Capabilities { get; private set; }

        /// <summary>
        /// List of errors associated with this device.
        /// </summary>
        public IDictionary<String, String> Errors { get; private set; }

        /// <summary>
        /// Gets the origin of the information about the device: cloud, cache or from recovery (???).
        /// </summary>
        public ResponseType ResponseOrigin { get; private set; }

        /// <summary>
        /// Gets the value of a specific retrieved capability.
        /// </summary>
        /// <param name="capabilityName">Name of the capability</param>
        /// <returns>Value of the capability</returns>
        public String Get(String capabilityName)
        {
            if (String.IsNullOrEmpty(capabilityName))
                return String.Empty;

            var result = Capabilities.ContainsKey(capabilityName) ? Capabilities[capabilityName] : String.Empty;
            return result;
        }

        /// <summary>
        /// Reports the version of the server cloud API.
        /// </summary>
        public String ServerVersion { get; internal set; }

        /// <summary>
        /// Date the WURFL was last updated on the server. 
        /// </summary>
        public DateTime WurflLastUpdate { get; internal set; }

        /// <summary>
        /// Reports the ID of the device as identified by the WURFL engine. 
        /// </summary>
        public String Id { get; internal set; }
    }
}