using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("host")]
    public partial class Host
    {
        [Key]
        public long host_id { get; set; }

        [Required]
        [StringLength(250)]
        public string name { get; set; }

        [Required]
        [StringLength(40)]
        public string address { get; set; }

        [Required]
        [StringLength(15)]
        public string record_status { get; set; }

        public DateTime created_date { get; set; }
    }
}
