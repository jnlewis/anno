using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class ClaimEarningsStatuses
    {
        public static string Success = "1";
        public static string EventNotFound = "10";
        public static string EventNotStarted = "11";
        public static string EventAlreadyClaimed = "12";
        public static string EventAlreadyCancelled = "13";
        public static string HostNotFound = "14";
    }
}