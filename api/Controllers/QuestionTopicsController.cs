using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Controllers.AuthUtil;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using api.Controllers.Params;

namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    public class QuestionTopicsController : Controller
    {
        private readonly FaqChatBotDbContext _context;
        private readonly JwtDecoder _jwtDecoder;

        public QuestionTopicsController(FaqChatBotDbContext context, JwtDecoder jwtDecoder)
        {
            _context = context;
            _jwtDecoder = jwtDecoder;
        }
        
        // GET: api/QuestionTopics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionTopic>>> GetQuestionTopics(
            [FromQuery] string sort, [FromQuery] string range, [FromQuery] string filter)
        {
            var sortParam = SortParam.ParseParam(sort) ;
            var rangeParam = RangeParam.ParseParam(range);
            var filterParam = FilterParam.ParseParam(filter);
            var query = _context.QuestionTopics
                .Where(t => !t.Deleted)
                .Filter(filterParam);
            var totalEntryCount = await query.CountAsync();
            query = query.OrderBy(sortParam).Range(rangeParam);
            var topicList = await query.AsNoTracking().ToListAsync();
            var count = topicList.Count;
            Response.Headers.AddContentRange("QuestionTopics", rangeParam, totalEntryCount, count);
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            return topicList;
        }

        

        // GET: api/QuestionTopics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionTopic>> GetQuestionTopic(int id)
        {
            var questionTopic = await _context.QuestionTopics.FindAsync(id);

            if (questionTopic == null || questionTopic.Deleted)
            {
                return NotFound();
            }

            return questionTopic;
        }

        // PUT: api/QuestionTopics/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<ActionResult<QuestionTopic>> PutQuestionTopic(int id, QuestionTopic questionTopic)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
            if (id != questionTopic.Id)
            {
                return BadRequest();
            }
            questionTopic.UpdateTime = DateTime.Now;
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

                throw;
            }

            return questionTopic;
        }

        // POST: api/QuestionTopics
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<QuestionTopic>> PostQuestionTopic( QuestionTopic questionTopic)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
            if (QuestionTopicExists(questionTopic.Id))
            {
                return BadRequest();
            }
            questionTopic.UpdateTime = DateTime.Now;
            _context.QuestionTopics.Add(questionTopic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestionTopic", new { id = questionTopic.Id }, questionTopic);
        }

        // DELETE: api/QuestionTopics/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<QuestionTopic>> DeleteQuestionTopic(int id)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
            var questionTopic = await _context.QuestionTopics.FindAsync(id);
            if (questionTopic == null || questionTopic.Deleted)
            {
                return NotFound();
            }

            questionTopic.Deleted = true;
            questionTopic.UpdateTime = DateTime.Now;
            await _context.SaveChangesAsync();
            return questionTopic;
        }


        [HttpGet("{topicId:int}/topQuestions/{number:int}")]
        public async Task<ActionResult<List<Question>>> TopQuestions(int topicId, int number)
        {
            if ((await _context.QuestionTopics.FindAsync(topicId)).Deleted)
            {
                return NotFound(); // topic not found
            }
            return await _context.Questions
                .Where(q => q.QuestionTopicId == topicId && !q.Deleted)
                .OrderBy(q => q.Rank)
                .Take(number)
                .ToListAsync();
        }


        private bool QuestionTopicExists(int id)
        {
            var topic = _context.QuestionTopics.Find(id);
            return topic != null && !topic.Deleted;
        }
        
        private bool AuthenticateEditor()
        {
            if (!_jwtDecoder.Verify(Request)) return false;
            var payload = JwtDecoder.ParsePayload(Request);
            if (payload.Roles.Contains(AdminUserRole.Admin)) return true;
            return false;
        }
    }
}
