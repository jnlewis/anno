using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class GetBookingsByEventResponse
    {
        public List<Booking> Bookings { get; set; }
    }
}