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
using System.Net;
using ScientiaMobile.WurflCloud.Utils;

namespace ScientiaMobile.WurflCloud.Config
{
    /// <summary>
    /// Stores configuration data for the cloud client. This class is not initialized; use DefaultCloudClientConfig.
    /// </summary>
    public class CloudClientConfig
    {
        /// <summary>
        /// Initializes the client configuration object.
        /// </summary>
        public CloudClientConfig()
        {
            // Ensures at the least the default server is added to the list.
            AvailableCloudServers = new List<CloudServerConfig>();
            AddCloudServer(new DefaultCloudServerConfig());
        }

        /// <summary>
        /// The WURFL cloud server currently in use
        /// </summary>
        protected CloudServerConfig CurrentServer { get; private set; }

        /// <summary>
        /// The API Key is used to authenticate with the WURFL Cloud Service. It can be found at in your account
        /// at http://www.scientiamobile.com/myaccount.
        /// The API Key is 39 characters in with the format: nnnnnn:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx 
        /// where 'n' is a number and 'x' is a letter or number
        /// </summary>
        public String ApiKey { get; set; }

        public WebProxy Proxy { get; set; }

        /// <summary>
        /// The timeouts on connections
        /// </summary>
        public Int32 ConnectionTimeout { get; set; }
        public Int32 ReadTimeout { get; set; }


        /// <summary>
        /// Indicates whether GZIP compression is required from cloud server. 
        /// </summary>
        public Boolean Compression { get; set; }

        /// <summary>
        /// Indicates the list of available servers for WURFL cloud requests that can't be served from the cache.
        /// </summary>
        //protected IDictionary<String, CloudServerConfig> AvailableCloudServers = new Dictionary<String, CloudServerConfig>();
        protected IList<CloudServerConfig> AvailableCloudServers;

        /// <summary>
        /// Empty the list of WURFL cloud servers.
        /// </summary> 
        public void ClearCloudServers()
        {
            AvailableCloudServers.Clear(); 
        }

        /// <summary>
        /// Add a new cloud server to the list.
        /// </summary>
        public void AddCloudServer(CloudServerConfig serverConfig)
        {
            // AvailableCloudServers.Add(serverConfig.Nickname, serverConfig);
            AvailableCloudServers.Add(serverConfig);
        }
        public void AddCloudServer(String nickname, String url, Int32 weight = Constants.DefaultServerWeight)
        {
            // AvailableCloudServers.Add(nickname, new CloudServerConfig(nickname, url, weight));
            AvailableCloudServers.Add(new CloudServerConfig(nickname, url, weight));
        }

        /// <summary>
        /// Determines the WURFL cloud server to be used. 
        /// </summary>
        /// <returns>Configuration of the server</returns>
        public CloudServerConfig GetServer()
        {
            return GetPreferredServer();
        }

        #region Helpers
        /// <summary>
        /// Uses a weight-based algorithm to pick up a server from the pool of available servers.
        /// </summary>
        /// <returns>Array of server configurations</returns>
        private CloudServerConfig GetPreferredServer()
        {
            if (CurrentServer == null)
            {
                if (AvailableCloudServers.Count == 1)
                    CurrentServer = AvailableCloudServers[0];
            }
            else
            {
                // Get total of weights in the array
                var total = 0;
                foreach (var serverConfig in AvailableCloudServers)
                {
                    total += serverConfig.Weight;
                }

                // Get a random number
                var sampleWeight = new Random().Next(total + 1);

                // Pick up the first server where sum of weights exceeds sample
                var weightSum = 0;
                foreach(var serverConfig in AvailableCloudServers)
                {
                    weightSum += serverConfig.Weight;
                    if (weightSum < sampleWeight) 
                        continue;
                    CurrentServer = serverConfig;
                    break;
                }
            }                

            return CurrentServer;
        }
        #endregion
    }
}