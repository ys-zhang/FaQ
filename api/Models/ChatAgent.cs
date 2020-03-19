using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class ChatAgent
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public ChatAgentType Type { get; set; }
        public string Avatar { get; set; }
    }

    public enum ChatAgentType
    {
        Bot, Human
    }
}