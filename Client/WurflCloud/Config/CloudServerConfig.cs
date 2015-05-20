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
    /// Provides an abstract description of a WURFL cloud server. 
    /// </summary>
    public class CloudServerConfig
    {
        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="nickname">Familiar name of the server</param>
        /// <param name="host">URL of the server</param>
        /// <param name="weight">Weight of the server</param>
        public CloudServerConfig(String nickname, String host, Int32 weight = Constants.DefaultServerWeight)
        {
            Host = host;
            Weight = weight;
            Nickname = nickname;
        }

        /// <summary>
        /// Familiar name of the server
        /// </summary>
        public String Nickname { get; set; }
        
        /// <summary>
        /// URL of the server
        /// </summary>
        public String Host { get; set; }

        /// <summary>
        /// Indicates the importance of this server.
        /// Can be any positive number; weights are relative to each other. 
        /// </summary>
        public Int32 Weight { get; set; }

        /// <summary>
        /// Returns a string representation of the content.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Format("{0} at {1}, Weight: {2}", Nickname, Host, Weight);
        }
    }
}