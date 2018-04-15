using System;
using System.Collections.Generic;

namespace Anno.Api.Models
{
    public class BookEventResponse
    {
        public string ConfirmationNumber { get; set; }
        public List<string> TicketNumbers { get; set; }
    }
}