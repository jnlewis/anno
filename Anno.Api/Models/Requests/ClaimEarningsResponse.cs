using System;

namespace Anno.Api.Models
{
    public class ClaimEarningsResponse
    {
        public string EventTitle { get; set; }
        public decimal? Earnings { get; set; }
    }
}