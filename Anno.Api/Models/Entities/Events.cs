using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("events")]
    public partial class Events
    {
        [Key]
        public long event_id { get; set; }

        public long host_id { get; set; }

        [Required]
        [StringLength(30)]
        public string ref_id { get; set; }

        [StringLength(200)]
        public string title { get; set; }

        [StringLength(1000)]
        public string description { get; set; }
        
        public Nullable<DateTime> start_date { get; set; }
        
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
