using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AnnoAPI.Core.Const
{
    public static class BookEventStatuses
    {
        public const string Success = "1";
        public const string CustomerNotFound = "8";
        public const string EventNotFound = "9";
        public const string TierNotFound = "10";
        public const string EventNotActive = "11";
        public const string EventHasAlreadyStarted = "12";
        public const string InsufficientTickets = "13";
        public const string CustomerInsufficientFunds = "14";
        public const string InvalidTicketQuantity = "15";
    }
}