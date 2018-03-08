using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("customer_ticket")]
    public partial class CustomerTicket
    {
        [Key]
        public long ticket_id { get; set; }

        public long customer_id { get; set; }

        public long booking_id { get; set; }

        public long event_id { get; set; }

        public long tier_id { get; set; }
        
        [Required]
        [StringLength(30)]
        public string ticket_number { get; set; }

        [StringLength(15)]
        public string seat_number { get; set; }

        public decimal paid_price { get; set; }

        [Required]
        [StringLength(30)]
        public string status { get; set; }

        [Required]
        [StringLength(40)]
        public string address { get; set; }

        [Required]
        [StringLength(15)]
        public string record_status { get; set; }

        public DateTime created_date { get; set; }
    }
}
