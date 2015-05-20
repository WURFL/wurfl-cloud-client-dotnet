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
    public interface IWurflCloudCache
    {
        /// <summary>
        /// Given a HTTP request, retrieves device capabilities from the current cache provider.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface of the request with headers and other information</param>
        /// <returns>Device information</returns>
        DeviceInfo GetDevice(IServiceProvider context);

        /// <summary>
        /// Given a user-agent string, retrieves device capabilities from the current cache provider.
        /// </summary>
        /// <param name="userAgent">User agent string</param>
        /// <returns>Device information</returns>
        DeviceInfo GetDevice(String userAgent);

        /// <summary>
        /// Adds device information to the current cache provider using request UA as the key.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface of the request with headers and other information</param>
        /// <param name="device">Device information</param>
        /// <returns></returns>
        Boolean SetDevice(IServiceProvider context, DeviceInfo device);

        /// <summary>
        /// Adds device information to the current cache provider using given UA as the key.
        /// </summary>
        /// <param name="userAgent">User agent string</param>
        /// <param name="device">Device information</param>
        /// <returns></returns>
        Boolean SetDevice(String userAgent, DeviceInfo device);

        /// <summary>
        /// Empty the cache deleting all devices and mtime. 
        /// </summary>
        /// <returns>True in case of success</returns>
        Boolean Purge();

        /// <summary>
        /// Closes the connection to the cache provider.
        /// </summary>
        void Close();

        /// <summary>
        /// Gets the last loaded WURFL timestamp from the cache provider.
        /// This is used to detect when a new WURFL database has been loaded on the server.
        /// </summary>
        /// <returns>Unix timestamp</returns>
        long GetMtime();

        /// <summary>
        /// Sets the last loaded WURFL timestamp in the cache provider
        /// </summary>
        /// <param name="mtime">Unix timestamp</param>
        /// <returns>True in case of success</returns>
        Boolean SetMtime(long mtime);

        /// <summary>
        /// Returns an array filled with all counters ???
        /// </summary>
        /// <returns>Counters</returns>
        Dictionary<String, long> GetCounters();

        /// <summary>
        /// Resets counters to zero.
        /// </summary>
        void ResetCounters();

        /// <summary>
        /// Returns the number of seconds since the counter report was last sent.
        /// </summary>
        /// <returns>Seconds</returns>
        long GetReportAge();

        /// <summary>
        /// Resets the report age to zero.
        /// </summary>
        void ResetReportAge();
    }
}