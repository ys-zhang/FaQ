using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace api.Models
{
    public class FAQChatBotDBContext : DbContext
    {
        public DbSet<Question> questions { get; set; }
        public DbSet<QuestionTopic> questionTopics { get; set; }

        public FAQChatBotDBContext(DbContextOptions<FAQChatBotDBContext> options) : base(options) { }

    }
}
