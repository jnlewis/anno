using System;

namespace AnnoAPI.Models
{
    public class CreateEventTier
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int? TotalTickets { get; set; }
        public long? Price { get; set; }
    }
}