using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using ScientiaMobile.WurflCloud.Config;
using ScientiaMobile.WurflCloud;
using ScientiaMobile.WurflCloud.Cache;


namespace ScientiaMobile.WurflCloudTest_NUnit
{
    [TestClass]
    public class CloudClientInvalidCapabilityTest
    {
        [TestMethod]
        public void testIinvalidCapability()
        {
            var ua = "Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en-US) AppleWebKit/534.8+ (KHTML, like Gecko) Version/6.0.0.466 Mobile Safari/534.8+";

            var config = new DefaultCloudClientConfig
            {
                ApiKey = "XXXXXX:YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY"
            };

            var cache = new NoWurflCloudCache();

            var manager = new CloudClientManager(config,cache);

            var capabilities = new string[] { "is_vireless_device" };

            WurflCloud.Device.DeviceInfo di = manager.GetDeviceInfo(ua, capabilities);

            var capError = false;
            string capValue;

            capError = di.Errors.TryGetValue("is_vireless_device", out capValue);

            Assert.IsTrue(capError);
        }
    }
}
