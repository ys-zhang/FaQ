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
        /// <summary>
        /// the message owns this option
        /// </summary>
        public int MessageId { get; set; }
        
        [ForeignKey("MessageId")]
        [JsonIgnore]
        public Message Message { get; set; }
    }
}