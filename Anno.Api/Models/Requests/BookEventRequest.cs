using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class BookEventRequest
    {
        public string CustomerReferenceId { get; set; }
        public string EventReferenceId { get; set; }
        public List<BookEventRequestTicket> Tickets { get; set; }
    }

    public class BookEventRequestTicket
    {
        public string EventTierReferenceId { get; set; }
        public int Quantity { get; set; }
    }
}