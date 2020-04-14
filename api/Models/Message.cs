using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace api.Models
{
    public class Message
    {
        [Key] 
        public int Id { get; set; }

        public string Description { get; set; }
        
        [JsonConverter(typeof(StringEnumConverter))]
        public AnswerType AnswerType { get; set; }

        [InverseProperty("Message")]
        public List<MessageContent> MessageContents { get; set; }

        [InverseProperty("Message")]
        public List<MessageOption> MessageOptions { get; set; }

    }

    /// <summary>
    /// type of answer that a message anticipates 
    /// </summary>
    public enum AnswerType
    {
        /// <summary>
        /// Costumer needs to select an option
        /// </summary>
        Option, 
        /// <summary>
        /// Require text input from costumer 
        /// </summary>
        Text, 
        /// <summary>
        /// Option & Text
        /// </summary>
        OptionText
    }
}