namespace api.Models
{
    public class MessageAndOptionRelation
    {
        public int MessageId { get; set; } 
        public int MessageOptionId { get; set; }
        public Message Message { get; set; }
        public MessageOption MessageOption { get; set; }
    }
}