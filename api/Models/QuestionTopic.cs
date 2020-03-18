using System;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class QuestionTopic: Updatable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string Icon { get; set; }
        public bool Active { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime updateTime { get; set; }
    }
}
