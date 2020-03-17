using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class QuestionTopic: Updatable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int Name { get; set; }
        public string Icon { get; set; }
        public bool Active { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime updateTime { get; set; }
    }
}
