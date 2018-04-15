using System;

namespace AnnoAPI.Models
{
    public class CreateCustomerResponse
    {
        public string WalletAddress { get; set; }
        public int? WalletBalance { get; set; }
    }
}