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
        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
    
    public enum MessageContentType
    {
        Text, Link, Image
    }
}