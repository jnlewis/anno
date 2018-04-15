using Anno.Models.Entities;
using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class GetEventDetailsByRefResponse
    {
        public EventInfo Event { get; set; }
        public List<EventTierInfo> Tiers { get; set; }
    }
}