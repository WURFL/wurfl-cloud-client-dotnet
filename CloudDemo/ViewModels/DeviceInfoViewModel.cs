using System;
using System.Collections.Generic;
using ScientiaMobile.WurflCloud.Http;

namespace CloudDemo.ViewModels
{
    public class DeviceInfoViewModel
    {
        public String UserAgent { get; set; }
        public String ServerVersion { get; set; }
        public String Library { get; set; }
        public String DeviceId { get; set; }
        public String DateOfRequest { get; set; }
        public String CachingModule { get; set; }
        public ResponseType Source { get; set; }
        public IDictionary<String, String> Capabilities { get; set; }
        public IDictionary<String, String> Errors { get; set; }
    }
}