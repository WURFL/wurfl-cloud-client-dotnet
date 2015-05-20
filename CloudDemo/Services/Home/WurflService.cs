using System;
using System.Web;
using CloudDemo.ViewModels;
using ScientiaMobile.WurflCloud;
using ScientiaMobile.WurflCloud.Cache;
using ScientiaMobile.WurflCloud.Config;
using ScientiaMobile.WurflCloud.Request;

namespace CloudDemo.Services.Home
{
    public class WurflService
    {
        public DeviceInfoViewModel GetDataByRequest(HttpContextBase context)
        {
            var config = new DefaultCloudClientConfig
            {
                ApiKey = "xxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            };

            var manager = new CloudClientManager(config).SetCache(new CookieWurflCloudCache());
            
            // Grab data
            var info = manager.GetDeviceInfo(context, new[] { "is_wireless_device", "release_date", "brand_name", "device_os" });
            var model = new DeviceInfoViewModel
                                {
                                    DeviceId = info.Id,
                                    ServerVersion = info.ServerVersion,
                                    DateOfRequest = info.WurflLastUpdate.ToString(),
                                    CachingModule = manager.GetCachingModuleName(),
                                    Library = manager.GetClientVersion(),
                                    Capabilities = info.Capabilities,
                                    Errors = info.Errors,
                                    Source = info.ResponseOrigin
                                };

            return model;
        }

        public DeviceInfoViewModel GetDataByAgent(HttpContextBase context, String ua)
        {
            var config = new DefaultCloudClientConfig
            {
                ApiKey = "xxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
            };

            var manager = new CloudClientManager(config).SetCache(new MemoryWurflCloudCache());
            var wurflRequest = new WurflCloudRequest(context) {UserAgent = ua};

            // Grab data
            var info = manager.GetDeviceInfo(wurflRequest, new[] { "is_wireless_device", "is_smartphone", "physical_screen_width" });
            var model = new DeviceInfoViewModel
                                {
                                    DeviceId = info.Id,
                                    UserAgent = ua,
                                    ServerVersion = info.ServerVersion,
                                    DateOfRequest = info.WurflLastUpdate.ToLongTimeString(),
                                    CachingModule = manager.GetCachingModuleName(),
                                    Library = manager.GetClientVersion(),
                                    Capabilities = info.Capabilities,
                                    Errors = info.Errors,
                                    Source = info.ResponseOrigin
                                };
            return model;
        }
    }
}