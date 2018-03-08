using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("transactions")]
    public partial class Transaction
    {
        [Key]
        public long transaction_id { get; set; }

        public DateTime transaction_datetime { get; set; }
    
        [StringLength(40)]
        public string address_from { get; set; }
        
        [StringLength(40)]
        public string address_to { get; set; }

        public decimal amount { get; set; }

        public Nullable<long> booking_id { get; set; }

        [Required]
        [StringLength(30)]
        public string description { get; set; }
        
        public DateTime created_date { get; set; }
    }
}
