using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Anno.Api.Core.Utility
{
    public static class DateTimeUtility
    {
        public static DateTime FromUnixTime(int unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }
        public static int ToUnixTime(DateTime timeStamp)
        {
            return (Int32)(timeStamp.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }
    }
}