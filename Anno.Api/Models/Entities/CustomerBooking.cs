using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("customer_booking")]
    public partial class CustomerBooking
    {
        [Key]
        public long booking_id { get; set; }

        public long customer_id { get; set; }

        public long event_id { get; set; }

        [Required]
        [StringLength(30)]
        public string confirmation_number { get; set; }
        
        [Required]
        [StringLength(15)]
        public string record_status { get; set; }

        public DateTime created_date { get; set; }
    }
}
