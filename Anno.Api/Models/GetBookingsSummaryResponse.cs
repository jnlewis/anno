using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class GetBookingsSummaryResponse
    {
        public List<BookingSummary> Bookings { get; set; }
    }
}