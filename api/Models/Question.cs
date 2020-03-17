using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public interface Updatable
    {
        // if deleted from the admin
        public bool Deleted { get; set; }
        public DateTime updateTime { get; set; }
        public bool Active { get; set; }
    }
    public class Question : Updatable
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
        public string Answer { get; set; }
        public int QuestionTopicId { get; set; }
        public int Rank { get; set; } = int.MaxValue;
        // if can be showed on FAQ
        public bool Active { get; set; } = false;
        public bool Deleted { get; set; } = false;
        public DateTime updateTime { get; set; }
        public QuestionTopic questionTopic { get; set; }
    }
}
