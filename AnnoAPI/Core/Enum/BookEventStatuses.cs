using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Enum
{
    public static class BookEventStatuses
    {
        public static string Success = "1";
        public static string CustomerNotFound = "8";
        public static string EventNotFound = "9";
        public static string TierNotFound = "10";
        public static string EventNotActive = "11";
        public static string EventHasAlreadyStarted = "12";
        public static string InsufficientTickets = "13";
        public static string CustomerInsufficientFunds = "14";
        public static string InvalidTicketQuantity = "15";
    }
}