using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Const
{
    public static class ClaimEarningsStatuses
    {
        public const string Success = "1";
        public const string EventNotFound = "10";
        public const string EventNotStarted = "11";
        public const string EventAlreadyClaimed = "12";
        public const string EventAlreadyCancelled = "13";
        public const string HostNotFound = "14";
    }
}