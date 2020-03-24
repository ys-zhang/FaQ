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
        [JsonConverter(typeof(StringEnumConverter))]
        public AnswerType AnswerType { get; set; }
        
        [NotMapped]
        public List<MessageContent> Contents
        {
            get => ContentRelations?.Select(r => r.MessageContent).ToList();
            set
            {
                ContentRelations = value?.Select(content => new MessageAndContentRelation
                {
                    MessageId = Id,
                    Message = this,
                    MessageContentId = content.Id,
                    MessageContent = content
                }).ToList();
            }
            
        }
        
        [NotMapped]
        public List<MessageOption> Options
        {
            get => OptionRelations?.Select(r => r.MessageOption).ToList();
            set
            {
                OptionRelations = value?.Select(option => new MessageAndOptionRelation
                {
                    MessageId = Id,
                    Message = this,
                    MessageOptionId = option.Id,
                    MessageOption = option
                }).ToList();
            }
        }

        [JsonIgnore]
        public IList<MessageAndContentRelation> ContentRelations { get; set; }
        [JsonIgnore]
        public IList<MessageAndOptionRelation> OptionRelations { get; set; }
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