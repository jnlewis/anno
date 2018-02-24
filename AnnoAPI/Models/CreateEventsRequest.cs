using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class CreateEventsRequest
    {
        public string ReferenceId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime? StartDate { get; set; }

        public List<CreateEventTier> Tiers { get; set; }
    }
}