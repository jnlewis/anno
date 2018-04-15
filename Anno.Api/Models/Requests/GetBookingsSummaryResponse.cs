using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class GetBookingsSummaryResponse
    {
        public List<BookingSummary> Bookings { get; set; }
    }
}