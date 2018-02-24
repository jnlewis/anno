using System;

namespace AnnoAPI.Models
{
    public class Ticket
    {
        public long? TicketId { get; set; }
        public long? BookingId { get; set; }
        public string CustomerReferenceId { get; set; }
        public string EventReferenceId { get; set; }
        public string TierReferenceId { get; set; }
        public string EventTitle { get; set; }
        public string TierTitle { get; set; }
        public string TicketNo { get; set; }
        public string BookingConfirmationNo { get; set; }
        public string Status { get; set; }
        public long? PaidPrice { get; set; }
        public string TicketAddress { get; set; }
    }
}