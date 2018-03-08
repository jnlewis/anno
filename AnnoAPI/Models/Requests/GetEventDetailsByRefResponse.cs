using Anno.Models.Entities;
using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class GetEventDetailsByRefResponse
    {
        public EventInfo Event { get; set; }
        public List<EventTierInfo> Tiers { get; set; }
    }
}