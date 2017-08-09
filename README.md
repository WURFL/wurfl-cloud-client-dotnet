# ScientiaMobile WURFL Cloud Client for .NET

The WURFL Cloud Service by ScientiaMobile, Inc., is a cloud-based
mobile device detection service that can quickly and accurately
detect over 500 capabilities of visiting devices.  It can differentiate
between portable mobile devices, desktop devices, SmartTVs and any 
other types of devices that have a web browser.

This is the .NET Client for accessing the WURFL Cloud Service, and
it requires a free or paid WURFL Cloud account from ScientiaMobile:
http://www.scientiamobile.com/cloud 

## Installation
--------------
### Requirements
 - `.NET 2.0+`
 - `Newtonsoft.Json 4.5`

### Sign up for WURFL Cloud
First, you must go to http://www.scientiamobile.com/cloud and signup
for a free or paid WURFL Cloud account (see above).  When you've finished
creating your account, you must copy your API Key, as it will be needed in
the Client.

### Get ScientiaMobile WURFL Cloud Client assembly
You can get ScientiaMobile WURFL Cloud Client assembly in two ways:
- by cloning this Github repo and building the *ScientiaMobile.WurflCloud* project from the *master* branch
- by installing it as a [NuGet package] (https://www.nuget.org/packages/Wurfl_Official_Cloud_API/)

In both cases you need to reference in your project the *ScientiaMobile.WurflCloud.dll* assembly along with
the third party *Newtonsoft.Json.dll* 

## Integration examples

### Simple Console application
Here's a quick example of how to get WurflCloud up and running in a Console Application.

In your Console Application project, add *ScientiaMobile.WurflCloud.dll* *Newtonsoft.Json.dll* assemblies as reference.

Add a new class to named *SimpleConsoleApplication* to your project with the following code.

```
using System;
using System.Collections.Generic;
using ScientiaMobile.WurflCloud;
using ScientiaMobile.WurflCloud.Config;
using ScientiaMobile.WurflCloud.Cache;
using ScientiaMobile.WurflCloud.Device;

namespace YourNameSpace
{
    class SimpleConsoleApplication
    {
        static void Main(string[] args)
        {
			// The User-Agent to detect
            var ua = "Mozilla/5.0 (BlackBerry; U; BlackBerry 9800; en-US) AppleWebKit/534.8+ (KHTML, like Gecko) Version/6.0.0.466 Mobile Safari/534.8+";

			// The WurflCloud configuration
            var config = new DefaultCloudClientConfig
            {
				// Your API Key
                ApiKey = "xxxxxx:yyyyyyyyyyyyyyyyyyyyyyyyyyyyyyyy"
            };

			// Configure CloudClientManager with no cache
            var cache = new NoWurflCloudCache();

            var manager = new CloudClientManager(config, cache);

			// Get all static capability provided by your license
            var capabilities = new string[0];

			// Perform device detection using provided User-Agent
            DeviceInfo di = manager.GetDeviceInfo(ua, capabilities);

			// Write the detected Device Id
            Console.WriteLine(di.Id);

			// Write static capabilities name/value
            foreach (KeyValuePair<string, string> entry in di.Capabilities)
            {
                Console.WriteLine(entry.Key + " " + entry.Value);
            }
			
			Console.ReadKey();
        }
    }
}
```

### Simple Web application
You should review the included example (CloudDemo) to get a feel for
the Client API, and how best to use it in your web application.

Here's a quick example of how to get up and running quickly:

In CloudDemo\Services\Home\WurflService.cs set your personal ApiKey
both in GetDataByRequest and GetDataByAgent methods.

GetDataByRequest is an example on how to get capabilities starting from the HttpRequest.

```c#
public DeviceInfoViewModel GetDataByRequest(HttpContext context)
{
  var config = new DefaultCloudClientConfig
  {
     ApiKey = "xxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  };
  var manager = new CloudClientManager(config);
  var info = manager.GetDeviceInfo(context, new[] { "is_wireless_device", "model_name" });
  var model = new DeviceInfoViewModel
  {
     DeviceId = info.Id,
     ServerVersion = info.ServerVersion,
     DateOfRequest = info.WurflLastUpdate.ToString(),
     Library = manager.GetClientVersion(),
     Capabilities = info.Capabilities,
     Errors = info.Errors,
     Source = info.ResponseOrigin
  };
  return model;
}
```

GetDataByAgent is an example on how to get capabilities starting from a User-Agent string

```c#
public DeviceInfoViewModel GetDataByAgent(HttpContextBase context, String ua)
{
  var config = new DefaultCloudClientConfig
  {
  	 ApiKey = "xxxxxx:xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
  };
  
  var manager = new CloudClientManager(config).SetCache(new MemoryWurflCloudCache());
  
  // Grab data
  var info = manager.GetDeviceInfo(ua, new[] { "is_wireless_device", "is_smartphone", "physical_screen_width" });
  var model = new DeviceInfoViewModel
  {
     DeviceId = info.Id,
     UserAgent = ua,
     ServerVersion = info.ServerVersion,
     DateOfRequest = info.WurflLastUpdate.ToLongTimeString(),
     Library = manager.GetClientVersion(),
     Capabilities = info.Capabilities,
     Errors = info.Errors,
     Source = info.ResponseOrigin
  };
  return model;
}
```

The `CloudDemo\ViewModels\DeviceInfoViewModel` is a helper class that
gathers information from the WURFL cloud client API and makes it ready
for display. 

You can directly check a capability as below:

```c#
var isMobileAsText = info.Get("is_wireless_device"); // Returns a string!
```

## Configuration
---------------
The public interface of the WURFL Cloud API is fairly simple and consists of a
single class, `Client\WurflCloud\CloudClientManager`. The members of this class are:
 - `GetApiVersion`: Gets a string with the version number of the WURFL cloud server
 API.
 - `GetCachingModuleName`: Gets a string with the fully qualified name of the .NET
 class that the WURFL cloud ASP.NET API uses to cache server responses.
 - `GetClientVersion`: Gets a string with the version number of the WURFL cloud
 ASP.NET client API.
 - `GetDeviceInfo`: Returns the values of the capabilities available for the
 requesting device or user-agent string.
 - `SetCache`: Allows replacing the default cache module that the ASP.NET client
 API will use to improve performance.

### Configuration
Before you can start using the `Client\WurflCloud\CloudClientManager`, 
you should initialize it by providing a valid API key and optionally a caching module. 
This information is collected in an instance of the `Client\WurflCloud\Config\CloudClientConfig`
class.  The library offers a default configuration object through the
`Client\WurflCloud\DefaultCloudClientConfig` class. Most of the time, all you
need to do is getting a new instance of this class and set your API key, as shown
below:

```c#
var config = new DefaultCloudClientConfig
  {
    ApiKey = "123456:xxxxxxxxxxxxxxxxxxxxxxxxxx"
  };
var manager = new CloudClientManager(config);
```

The public members of the `CloudClientConfig` class are:
 - `ApiKey`: Gets and sets the API key that identifies your developer account
 - `Compression`: Indicates whether GZIP compression is required from cloud server.
Compression is disabled by default.

### Cache
The ASP.NET cloud API sits in between your ASP.NET application and the WURFL
cloud. It tracks all of your requests and caches frequently requested devices. 
The library provides full control over the caching infrastructure.
Caching modules available:

 - `Client/WurflCloud/Cache/CookieWurflCloudCache`: Uses a cookie to store the
 value of returned capabilities.
 - `Client/WurflCloud/Cache/MemoryWurflCloudCache`: Server responses are cached
 in the ASP.NET Cache object. The cached data has no dependencies and expires
 automatically if not used for 20 minutes.
 - `Client/WurflCloud/Cache/NoWurflCloudCache`: No caching layer is used and
 every request to the ASP.NET WURFLcloud client results in a request to the
 server cloud. The cache API has been designed to be pluggable. A valid cache
 module is any class that implements the `Client\WurflCloud\Cache\IWurflCloudCache`
 interface. For your convenience, the API also provides a half-done cache provider
 class—the `Client\WurflCloud\Cache\WurflCloudCacheBase`. The simplest and
 quickest way to create a custom cache module is deriving a new class from
 `Client\WurflCloud\Cache\WurflCloudCacheBase`. The class
 `CloudDemo\Common\FlexibleMemoryWurflCloudCache` provides an example that
 binds the response for any requested user agent string to a helper cache
 entry. By invalidating the helper cache entry, you can clear all cached
 data in a single shot.

## Querying the Cloud Client API
--------------
To query the WURFL database in the cloud, you use the GetDeviceInfo method on the 
`Client\WurflCloud\CloudClientManager` class. 
The method has a few overloads:
```c#
public DeviceInfo GetDeviceInfo(IServiceProvider context)
public DeviceInfo GetDeviceInfo(IServiceProvider context, String[] capabilities)
public DeviceInfo GetDeviceInfo(WurflCloudRequest request)
public DeviceInfo GetDeviceInfo(WurflCloudRequest request, String[] capabilities)
```
All methods return a `Client\WurflCloud\Device\DeviceInfo` object, whose public
properties are:

 - `Capabilities`: Name/value dictionary of capabilities as returned by the server.
 - `Errors`: Name/value dictionary of errors as returned by the server.
 - `Get`: Method that takes a capability name and returns the corresponding value
 as a string.
 - `Id`: ID of the device corresponding to the user agent string as identified by
 the WURFL server engine.
 - `ResponseOrigin`: Indicates the source of the response. Possible values come
 from the `Client\WurflCloud\Http\ResponseType` enumeration: None, Cloud, Cache.
 - `ServerVersion`: Version of the WURFL server API.
 - `WurflLastUpdate`: Date the WURFL database was last updated on the server.


**2015 ScientiaMobile Incorporated**

**All Rights Reserved.**

**NOTICE**:  All information contained herein is, and remains the property of
ScientiaMobile Incorporated and its suppliers, if any.  The intellectual
and technical concepts contained herein are proprietary to ScientiaMobile
Incorporated and its suppliers and may be covered by U.S. and Foreign
Patents, patents in process, and are protected by trade secret or copyright
law. Dissemination of this information or reproduction of this material is
strictly forbidden unless prior written permission is obtained from 
ScientiaMobile Incorporated.
