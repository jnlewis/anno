using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class GetBookingsByCustomerResponse
    {
        public List<Booking> Bookings { get; set; }
    }
}