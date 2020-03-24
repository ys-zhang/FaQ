namespace api.Models
{
    public class MessageAndContentRelation
    {
        public int MessageId { get; set; }
        public Message Message { get; set; }
        public int MessageContentId { get; set; }
        public MessageContent MessageContent { get; set; }
    }
}