using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace api.Models
{
    public class MessageContent
    {
        [Key]
        public int Id { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public MessageContentType Type { get; set; }
        public string Value { get; set; }
        [JsonIgnore]
        public IEnumerable<MessageAndContentRelation> Relations { get; set; }
        [JsonIgnore]
        public List<Message> RelatedMessages
            => Relations.Select(r => r.Message).ToList();
    }
    
    public enum MessageContentType
    {
        Text, Link, Image
    }
}