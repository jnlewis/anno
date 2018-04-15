using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class GetBookingsByCustomerResponse
    {
        public List<Booking> Bookings { get; set; }
    }
}