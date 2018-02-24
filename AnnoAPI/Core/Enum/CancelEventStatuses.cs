using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class CancelEventStatuses
    {
        public static string Success = "1";
        public static string EventNotFound = "9";
        public static string EventNotActive = "11";
        public static string EventHasAlreadyStarted = "12";
    }
}