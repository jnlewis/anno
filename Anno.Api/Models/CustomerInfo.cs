using System;

namespace AnnoAPI.Models
{
    public class CustomerInfo
    {
        public long? CustomerId { get; set; }
        public string RefId { get; set; }
        public string CustomerAddress { get; set; }
        public decimal? WalletBalance { get; set; }
        public string WalletAddress { get; set; }
    }
}