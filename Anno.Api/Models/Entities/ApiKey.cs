using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Anno.Models.Entities
{
    [Table("api_keys")]
    public partial class ApiKey
    {
        [Key]
        public long key_id { get; set; }

        public long host_id { get; set; }

        [Required]
        [StringLength(50)]
        public string api_key { get; set; }

        [Required]
        [StringLength(15)]
        public string record_status { get; set; }

        public DateTime created_date { get; set; }
    }
}
