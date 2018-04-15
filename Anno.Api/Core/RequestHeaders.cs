using System;
using System.Configuration;
using System.Web;

namespace AnnoAPI.Core
{
    public static class RequestHeaders
    {
        public static string API_KEY
        {
            get { return GetHeader("X-API-Key"); }
            set { SetHeader("X-API-Key", value); }
        }

        private static string GetHeader(string key)
        {
            return HttpContext.Current.Request.Headers[key];
        }

        private static void SetHeader(string key, string value)
        {
            if (HttpContext.Current.Request.Headers[key] != null)
                HttpContext.Current.Request.Headers.Remove(key);
            
            HttpContext.Current.Request.Headers.Add(key, value);
        }
    }
}