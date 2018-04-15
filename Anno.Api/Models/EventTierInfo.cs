using System;

namespace AnnoAPI.Models
{
    public class EventTierInfo
    {
        public long? TierId { get; set; }
        public long? EventId { get; set; }
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? TotalTickets { get; set; }
        public int? AvailableTickets { get; set; }
        public decimal? Price { get; set; }
        public string Status { get; set; }
        public string TierAddress { get; set; }
    }
}