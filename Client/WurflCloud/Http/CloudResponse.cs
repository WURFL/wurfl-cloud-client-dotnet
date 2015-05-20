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

namespace ScientiaMobile.WurflCloud.Http
{
    /// <summary>
    /// Represents the response you get directly from the WURFL cloud.
    /// The output from the cloud is a JSON string that is loaded into this type.
    /// </summary>
    public class CloudResponse
    {
        public CloudResponse()
        {
            Capabilities = new Dictionary<String, String>();
            Errors = new Dictionary<String, String>();
        }

        /// <summary>
        /// Version of the cloud API.
        /// </summary>
        public String ApiVersion { get; set; }

        /// <summary>
        /// Value that represents the time on the server as a Unix timestamps (seconds since Epoch???).
        /// </summary>
        public long MTime { get; set; }

        /// <summary>
        /// ??? Device ID ???
        /// </summary>
        public String Id { get; set; }

        /// <summary>
        /// Name/value dictionary of detected capabilities.
        /// </summary>
        public IDictionary<String, String> Capabilities { get; set; }

        /// <summary>
        /// List of errors detected: capability/error message
        /// </summary>
        public IDictionary<String, String> Errors { get; set; }
    }
}