using System;

namespace Anno.Api.Models
{
    public class EventsRequest
    {
        public string Category { get; set; }
        public int? StartIndex { get; set; }
        public int? EndIndex { get; set; }
    }
}