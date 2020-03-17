using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;

namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    public class QuestionTopicsController : ControllerBase
    {
        private readonly FAQChatBotDBContext _context;

        public QuestionTopicsController(FAQChatBotDBContext context)
        {
            _context = context;
        }

        // GET: api/QuestionTopics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionTopic>>> GetquestionTopics()
        {
            return await _context.questionTopics.ToListAsync();
        }

        // GET: api/QuestionTopics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionTopic>> GetQuestionTopic(int id)
        {
            var questionTopic = await _context.questionTopics.FindAsync(id);

            if (questionTopic == null)
            {
                return NotFound();
            }

            return questionTopic;
        }

        // PUT: api/QuestionTopics/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestionTopic(int id, QuestionTopic questionTopic)
        {
            if (id != questionTopic.Id)
            {
                return BadRequest();
            }

            _context.Entry(questionTopic).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionTopicExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/QuestionTopics
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<QuestionTopic>> PostQuestionTopic(QuestionTopic questionTopic)
        {
            _context.questionTopics.Add(questionTopic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestionTopic", new { id = questionTopic.Id }, questionTopic);
        }

        // DELETE: api/QuestionTopics/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<QuestionTopic>> DeleteQuestionTopic(int id)
        {
            var questionTopic = await _context.questionTopics.FindAsync(id);
            if (questionTopic == null)
            {
                return NotFound();
            }

            _context.questionTopics.Remove(questionTopic);
            await _context.SaveChangesAsync();

            return questionTopic;
        }


        [HttpGet("{topicId:int}/topQuestions/{number:int}")]
        public async Task<ActionResult<List<Question>>> TopQuestions(int topicId, int number)
        {
            if ((await _context.questionTopics.FindAsync(topicId)).Deleted)
            {
                return NotFound(); // topic not found
            }
            return await _context.questions
                .TakeWhile(q => q.QuestionTopicId == topicId && !q.Deleted)
                .OrderBy(q => q.Rank)
                .Take(number)
                .ToListAsync();
        }


        private bool QuestionTopicExists(int id)
        {
            var topic = _context.questionTopics.Find(id);
            if (topic == null || topic.Deleted) {
                return false;
            }
            return true;
        }
    }
}
