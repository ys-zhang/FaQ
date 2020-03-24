using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace api.Models
{
    public class MessageOption
    {
        [Key] public int Id { get; set; }
        public string Hint { get; set; }
        public int? NextMessageId { get; set; }
        [ForeignKey("NextMessageId")]
        [JsonIgnore]
        public Message NextMessage { get; set; }
        [JsonIgnore]
        public IEnumerable<MessageAndOptionRelation> IncludedInMessages { get; set; }
    }
}