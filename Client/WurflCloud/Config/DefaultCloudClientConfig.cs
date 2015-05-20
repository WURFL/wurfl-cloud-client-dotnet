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
using ScientiaMobile.WurflCloud.Utils;

namespace ScientiaMobile.WurflCloud.Config
{
    /// <summary>
    /// Default configuration using a demo key and default compression settings
    /// </summary>
    public class DefaultCloudClientConfig : CloudClientConfig
    {
        public DefaultCloudClientConfig(
            String key = Constants.DemoKey, 
            Boolean enableCompression = Constants.DefaultCompression)
        {
            ApiKey = key;
            Compression = enableCompression;
        }
    }
}