using System;

namespace AnnoAPI.Models
{
    public class BookingSummary
    {
        public string EventReferenceId { get; set; }
        public string TierReferenceId { get; set; }
        public string EventTitle { get; set; }
        public string TierTitle { get; set; }
        public int? TicketsSoldCount { get; set; }
    }
}