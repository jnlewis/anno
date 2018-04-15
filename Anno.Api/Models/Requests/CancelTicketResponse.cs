using System;

namespace Anno.Api.Models
{
    public class CancelTicketResponse
    {
        public string EventTitle { get; set; }
        public string TierTitle { get; set; }
        public decimal? PaidPrice { get; set; }
        public string NewTicketStatus { get; set; }
    }
}