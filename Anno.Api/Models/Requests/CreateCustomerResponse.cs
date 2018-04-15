using System;

namespace Anno.Api.Models
{
    public class CreateCustomerResponse
    {
        public string WalletAddress { get; set; }
        public int? WalletBalance { get; set; }
    }
}