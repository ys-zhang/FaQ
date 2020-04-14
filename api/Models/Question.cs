using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace api.Models
{
    public interface IUpdatable
    {
        // if deleted from the admin
        [JsonIgnore]
        public bool Deleted { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool Active { get; set; }
    }
    public class Question : IUpdatable
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
        [JsonIgnore]
        public bool Deleted { get; set; } = false;
        public DateTime UpdateTime { get; set; }
        public QuestionTopic QuestionTopic { get; set; }
    }
}
