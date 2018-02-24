using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class GetEventDetailsByRefResponse
    {
        public Event Event { get; set; }
        public List<EventTier> Tiers { get; set; }
    }
}