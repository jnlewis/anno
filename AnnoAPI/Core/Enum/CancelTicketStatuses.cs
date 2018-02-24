using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class CancelTicketStatuses
    {
        public static string Success = "1";
        public static string TicketNotFound = "10";
        public static string TicketAlreadyCancelled = "11";
        public static string TicketAlreadyUsed = "12";
        public static string EventNotFound = "13";
        public static string EventNotActive = "14";
        public static string EventHasAlreadyStarted = "15";
        public static string TierNotFound = "16";
        public static string CustomerNotFound = "17";
    }
}