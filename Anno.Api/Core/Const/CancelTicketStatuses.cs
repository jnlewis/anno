using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Const
{
    public static class CancelTicketStatuses
    {
        public const string Success = "1";
        public const string TicketNotFound = "10";
        public const string TicketAlreadyCancelled = "11";
        public const string TicketAlreadyUsed = "12";
        public const string EventNotFound = "13";
        public const string EventNotActive = "14";
        public const string EventHasAlreadyStarted = "15";
        public const string TierNotFound = "16";
        public const string CustomerNotFound = "17";
    }
}