using System;

namespace Anno.Api.Models
{
    public class HostInfo
    {
        public long? HostId { get; set; }
        public string Name { get; set; }
        public string WalletAddress { get; set; }
        public decimal? WalletBalance { get; set; }
    }
}