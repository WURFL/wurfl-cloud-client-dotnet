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
    public class CloudClientAcceptEncodingHeadersTest
    {
        [TestMethod]
        public void testAcceptEncoding()
        {
            var ua = "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; Touch; rv:11.0) like Gecko";

            var config = new DefaultCloudClientConfig
            {
                ApiKey = "XXXXXX:YYYYYYYYYYYYYYYYYYYYYYYYYYYYYYYY",
                Compression = true
            };

            var cache = new NoWurflCloudCache();

            var manager = new CloudClientManager(config,cache);

            var capabilities = new string[0];

            WurflCloud.Device.DeviceInfo di = manager.GetDeviceInfo(ua, capabilities);

            String cap;
            di.Capabilities.TryGetValue("form_factor", out cap);

            Assert.AreEqual(0, di.Errors.Count);
            Assert.AreNotEqual<String>("Robot",cap);
        }
    }
}
