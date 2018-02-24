using System;

namespace AnnoAPI.Models
{
    public class UpdateEventStatusRequest
    {
        public string ReferenceId { get; set; }
        public string NewStatus { get; set; }
    }
}