using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace api.Models
{
    public class StaticChatBotMessage
    {
        [Key] public int Id { get; set; }
        public IEnumerable<StaticChatBotMessageAndContentRelation> Relations { get; set; }
        public List<MessageContent> Contents 
            => Relations.Select(r => r.MessageContent).ToList();
        public AnswerType AnswerType { get; set; }
        public List<StaticChatBotMessageOption> Options { get; set; }
    }

    public class StaticChatBotMessageOption
    {
        [Key] public int Id { get; set; }
        public string Hint { get; set; }
        public int? NextStaticChatBotMessageId { get; set; }
        [ForeignKey("NextStaticChatBotMessageId")]
        public StaticChatBotMessage NextStaticChatBotMessage { get; set; }
    }

    public class StaticChatBotMessageAndContentRelation
    {
        public int StaticChatBotMessageId { get; set; }
        public StaticChatBotMessage StaticChatBotMessage { get; set; }
        public int MessageContentId { get; set; }
        public MessageContent MessageContent { get; set; }
    }
    
    public class MessageContent
    {
        [Key]
        public int Id { get; set; }
        public MessageContentType Type { get; set; }
        public string Value { get; set; }
        public IEnumerable<StaticChatBotMessageAndContentRelation> Relations { get; set; }
        public List<StaticChatBotMessage> RelatedMessages
            => Relations.Select(r => r.StaticChatBotMessage).ToList();
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

    public enum MessageContentType
    {
        Text, Link, Image
    }
    
}