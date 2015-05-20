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
using System.Collections.Specialized;
using System.Web;

namespace ScientiaMobile.WurflCloud.Request
{
    public class WurflCloudRequest
    {
        private readonly Dictionary<String, String> _requestHeaders;
        private readonly IServiceProvider _context;

        /// <summary>
        /// Instantiates a custom request object that provides input for device detection.
        /// </summary>
        /// <param name="context">Implementation of IServiceProvider Interface</param>
        public WurflCloudRequest(IServiceProvider externalContext)
        {
            _context = externalContext;

            var context = new GeneralHttpContext(externalContext);

            // Copy input headers into the internal collection
            _requestHeaders = new Dictionary<String, String>();
            var request = context.Request;
            if (request == null)
                return;
            
            UserAgent = request.UserAgent ?? String.Empty;

            var headers = context.Request.Headers;
            for (var i = 0; i < headers.Count; i++)
            {
                var key = headers.GetKey(i);
                var value = headers.Get(i);
                AddRequestHeader(key, value);
            }
        }

        /// <summary>
        /// Gets and sets the user agent. This setting overrides the 
        /// user-agent coming with the HTTP request.
        /// </summary>
        public String UserAgent { get; set; }

        /// <summary>
        /// Gets the list of headers to be used for determining device information.
        /// By default, the dictionary is populated from the HTTP request. 
        /// </summary>
        public Dictionary<String, String> Headers
        {
            get { return _requestHeaders; }
        }

        /// <summary>
        /// Gets the Implementation of IServiceProvider Interface of the request.
        /// </summary>
        public IServiceProvider HttpContext
        {
            get { return _context; }
        }

        #region General Structs for Iservice implementations
        internal class GeneralRequest
        {
            internal String UserAgent { get; private set; }
            internal NameValueCollection Headers { get; private set; }
            internal GeneralRequest(object request)
            {
                Headers = request.GetType().GetProperty("Headers").GetValue(request, null) as NameValueCollection;
                UserAgent = request.GetType().GetProperty("UserAgent").GetValue(request, null) as String;
            }

        }

        internal class GeneralHttpContext
        {
            internal GeneralRequest Request { get; private set; }
            internal GeneralHttpContext(IServiceProvider serviceProvider)
            {
                Request = new GeneralRequest(serviceProvider.GetType().GetProperty("Request").GetValue(serviceProvider, null));
            }
        }
        #endregion

        #region Helpers
        private void AddRequestHeader(String headerName, String headerValue)
        {
            _requestHeaders[headerName] = headerValue;
        }
        #endregion
    }
}