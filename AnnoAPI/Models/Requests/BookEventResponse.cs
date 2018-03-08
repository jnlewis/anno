using System;
using System.Collections.Generic;

namespace AnnoAPI.Models
{
    public class BookEventResponse
    {
        public string ConfirmationNumber { get; set; }
        public List<string> TicketNumbers { get; set; }
    }
}