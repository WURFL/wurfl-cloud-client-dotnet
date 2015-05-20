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

namespace ScientiaMobile.WurflCloud.Cache
{
    internal class CookieContent
    {
        public String ServerVersion { get; set; }
        public String ServerTimestamp { get; set; }
        public String DeviceId { get; set; }
        public DateTime DateOfCreation { get; set; }
        public IDictionary<String, String> Capabilities { get; set; }
    }
}