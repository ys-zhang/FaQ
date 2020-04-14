using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace api.Models
{
    public class QuestionTopic: IUpdatable
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public TitledImage Icon { get; set; }
        public bool Active { get; set; } = false;
        [JsonIgnore]
        public bool Deleted { get; set; } = false;
        public DateTime UpdateTime { get; set; }
    }

    public class TitledImage
    {
        public string Url { get; set; }
        public string Desc { get; set; }
        public override string ToString()
        {
            return $"{Url} ; {Desc}";
        }
        public static TitledImage Parse(string s)
        {
            var repr = s.Split(" ; ");
            var url = repr[0].Trim();
            var desc = repr.Length == 1 ? null : repr[1].Trim();
            return new TitledImage {Url = url, Desc = desc};
        }
    } 
}
