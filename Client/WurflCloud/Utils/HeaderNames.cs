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
    public class HeaderNames
    {
        /// <summary>
        /// User agent header name.
        /// </summary>
        public const String UserAgent = "User-Agent";

        /// <summary>
        /// Accept header name.
        /// </summary>
        public const String Accept = "Accept";

        /// <summary>
        /// Accept-Encoding header name.
        /// </summary>
        public const String AcceptEncoding = "Accept-Encoding";

        /// <summary>
        /// Authorization header name.
        /// </summary>
        public const String Authorization = "Authorization";

        /// <summary>
        /// Connection header name.
        /// </summary>
        public const String Connection = "Connection";

        /// <summary>
        /// Custom header: used to request cloud counters.
        /// </summary>
        public const String XCloudCounters = "X-Cloud-Counters";

        /// <summary>
        /// Custom header: used to identify the cloud client.
        /// </summary>
        public const String XCloudClient = "X-Cloud-Client";

        /// <summary>
        /// Content-Encoding header name.
        /// </summary>
        public const String ContentEncoding = "Content-Encoding";

        /// <summary>
        /// Remote_Addr header name.
        /// </summary>
        public const String RemoteAddr = "Remote_Addr";

        /// <summary>
        /// Http_X_Forwarded_For header name.
        /// </summary>
        public const String HttpXForwardedFor = "Http_X_Forwarded_For";

        /// <summary>
        /// Custom header: used to pass info about the requesting IP 
        /// </summary>
        public const String XForwardedFor = "X-Forwarded-For";

        /// <summary>
        /// Custom header: original Accept-Encoding header 
        /// </summary>
        public const String XAcceptEncoding = "X-Accept-Encoding";
    }
}