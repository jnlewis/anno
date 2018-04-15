using System;

namespace AnnoAPI.Models
{
    public class EventsRequest
    {
        public string Category { get; set; }
        public int? StartIndex { get; set; }
        public int? EndIndex { get; set; }
    }
}