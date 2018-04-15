using System;

namespace Anno.Api.Models
{
    public class UpdateEventStatusRequest
    {
        public string ReferenceId { get; set; }
        public string NewStatus { get; set; }
    }
}