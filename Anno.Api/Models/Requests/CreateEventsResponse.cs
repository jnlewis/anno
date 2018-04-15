using System;

namespace AnnoAPI.Models
{
    public class CreateEventsResponse
    {
        public string Category { get; set; }
        public int? StartIndex { get; set; }
        public int? EndIndex { get; set; }
    }
}