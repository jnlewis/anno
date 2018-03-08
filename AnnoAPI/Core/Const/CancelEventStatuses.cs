using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Const
{
    public static class CancelEventStatuses
    {
        public const string Success = "1";
        public const string EventNotFound = "9";
        public const string EventNotActive = "11";
        public const string EventHasAlreadyStarted = "12";
    }
}