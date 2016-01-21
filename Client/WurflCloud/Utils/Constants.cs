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

namespace ScientiaMobile.WurflCloud.Utils
{
    /// <summary>
    /// Global repository of constants values used throughout the library.
    /// </summary>
    public class Constants
    {
        /// <summary>
        /// The version of the current WURFL cloud client manager.
        /// </summary>
        public const String ClientVersion = "WurflCloudClient/.NET_1.3.0.1";

        /// <summary>
        /// The version of the current WURFL cloud client manager.
        /// </summary>
        public const String ServerVersion = "Unknown";

        /// <summary>
        /// The nickname of the default WURFL cloud host. 
        /// </summary>
        public const String DefaultServerNickname = "wurfl_cloud";

        /// <summary>
        /// The default host name for the WURFL cloud.
        /// </summary>
        public const String DefaultServerHost = "api.wurflcloud.com";

        /// <summary>
        /// The default weight for a WURFL cloud server.
        /// </summary>
        public const Int32 DefaultServerWeight = 100;

        /// <summary>
        /// File that contains information about "recovery devices" (???) 
        /// </summary>
        public const String RecoveryJsonFile = "/recovery.json";

        /// <summary>
        /// Auto-purge (???) is not enabled by default
        /// </summary>
        public const Boolean DefaultAutoPurge = false;

        /// <summary>
        /// Compression is enabled by default.
        /// </summary>
        public const Boolean DefaultCompression = true;

        /// <summary>
        /// Interval in seconds between two successive reports (of what???) from the cloud. When this 
        /// interval expires the next request contains more data (called report ...)
        /// </summary>
        public const Int32 DefaultReportInterval = 60;

        /// <summary>
        /// Security key used for demo purposes.
        /// </summary>
        public const String DemoKey = "100000:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx";

        /// <summary>
        /// Default timeout values on connections
        /// </summary>
        public const Int32 DefaultConnectionTimeout = 5000;
        public const Int32 DefaultReadTimeout = 10000;
    }
}