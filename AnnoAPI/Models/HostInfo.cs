using System;

namespace AnnoAPI.Models
{
    public class HostInfo
    {
        public long? HostId { get; set; }
        public string Name { get; set; }
        public string WalletAddress { get; set; }
        public long? WalletBalance { get; set; }
    }
}