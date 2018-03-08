using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("wallet")]
    public partial class Wallet
    {
        [Key]
        public long wallet_id { get; set; }

        public long owner_id { get; set; }

        [StringLength(10)]
        public string owner_type { get; set; }

        [StringLength(40)]
        public string address { get; set; }
        
        public decimal balance { get; set; }
        
        [Required]
        [StringLength(15)]
        public string record_status { get; set; }

        public DateTime created_date { get; set; }
    }
}
