using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace api.Models
{
    public class FaqChatBotDbContext : DbContext
    {
        public DbSet<Question> Questions { get; set; }
        public DbSet<QuestionTopic> QuestionTopics { get; set; }
        public DbSet<ChatAgent> ChatAgents { get; set; }
        public DbSet<StaticChatBotMessage> StaticChatBotMessages { get; set; }
        public DbSet<MessageContent> MessageContents { get; set; }

        public FaqChatBotDbContext(DbContextOptions<FaqChatBotDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>().Property(q => q.updateTime).HasDefaultValueSql("GetDate()");
            modelBuilder.Entity<QuestionTopic>().Property(t => t.updateTime).HasDefaultValueSql("GetDate()");
            modelBuilder.Entity<ChatAgent>().Property(agent => agent.Type)
                .HasConversion<string>(v => v.ToString(), s => Enum.Parse<ChatAgentType>(s));
            modelBuilder.Entity<QuestionTopic>().Property(topic => topic.Icon)
                .HasConversion<string>(v => v.ToString(), s => TitledImage.Parse(s));
            
            modelBuilder.Entity<StaticChatBotMessage>().Property(message => message.AnswerType)
                .HasConversion<string>(v => v.ToString(), s => Enum.Parse<AnswerType>(s));

            modelBuilder.Entity<StaticChatBotMessageAndContentRelation>()
                .HasKey(r => new {r.StaticChatBotMessageId, r.MessageContentId});
            modelBuilder.Entity<StaticChatBotMessageAndContentRelation>()
                .HasOne(r => r.StaticChatBotMessage)
                .WithMany(message => message.Relations);
            modelBuilder.Entity<StaticChatBotMessageAndContentRelation>()
                .HasOne(r => r.MessageContent)
                .WithMany(content => content.Relations);
        }
        
        public int CountByRawSql(string sql, params KeyValuePair<string, object>[] parameters)
        {
            int result = -1;
            SqlConnection connection = Database.GetDbConnection() as SqlConnection;

            try
            {
                connection.Open();

                using (SqlCommand command = connection.CreateCommand())
                {
                    command.CommandText = sql;

                    foreach (KeyValuePair<string, object> parameter in parameters)
                        command.Parameters.AddWithValue(parameter.Key, parameter.Value);

                    using (DbDataReader dataReader = command.ExecuteReader())
                        if (dataReader.HasRows)
                            while (dataReader.Read())
                                result = dataReader.GetInt32(0);
                }
            }

            // We should have better error handling here
            catch (System.Exception e)
            {
                throw e;
            }

            finally { connection.Close(); }

            return result;
        }
    }
}
