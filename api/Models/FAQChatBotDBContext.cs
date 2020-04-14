using System;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    public class FaqChatBotDbContext : DbContext
    {
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionTopic> QuestionTopics { get; set; }
        public DbSet<ChatAgent> ChatAgents { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }
        public DbSet<MessageOption> MessageOptions { get; set; }


        public FaqChatBotDbContext(DbContextOptions<FaqChatBotDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>().Property(q => q.UpdateTime).HasDefaultValueSql("GetDate()");
            modelBuilder.Entity<QuestionTopic>().Property(t => t.UpdateTime).HasDefaultValueSql("GetDate()");
            modelBuilder.Entity<ChatAgent>().Property(agent => agent.Type)
                .HasConversion(v => v.ToString(), s => Enum.Parse<ChatAgentType>(s));
            modelBuilder.Entity<QuestionTopic>().Property(topic => topic.Icon)
                .HasConversion(v => v.ToString(), s => TitledImage.Parse(s));

            modelBuilder.Entity<Message>().Property(message => message.AnswerType)
                .HasConversion(v => v.ToString(), s => Enum.Parse<AnswerType>(s));
            modelBuilder.Entity<MessageContent>().Property(c => c.Type)
                .HasConversion(v => v.ToString(), s => Enum.Parse<MessageContentType>(s));
        }
    }
}
