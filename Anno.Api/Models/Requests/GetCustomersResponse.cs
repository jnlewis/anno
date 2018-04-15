using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class GetCustomersResponse
    {
        public List<CustomerInfo> Customers { get; set; }
    }
}