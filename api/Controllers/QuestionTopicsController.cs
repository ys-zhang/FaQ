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
        private readonly FaqChatBotDbContext _context;

        public QuestionTopicsController(FaqChatBotDbContext context)
        {
            _context = context;
        }

        // GET: api/QuestionTopics
        [HttpGet]
        public async Task<ActionResult<IEnumerable<QuestionTopic>>> GetQuestionTopics(
            [FromQuery] string sort, [FromQuery] string range, [FromQuery] string filter)
        {
            var sortParam = SortParam.ParseParam(sort) ;
            var rangeParam = RangeParam.ParseParam(range);
            var filterParam = FilterParam.ParseParam(filter);
            var query = "SELECT * FROM QuestionTopics WHERE Deleted = 0";
            var countQuery = "SELECT count(*) FROM QuestionTopics WHERE Deleted = 0";
            if (filterParam != null)
            {
                if (filterParam.Values.Count == 1)
                {
                    query += $" AND {filterParam.Column} = {filterParam.Values[0]} "; 
                    countQuery += $" AND {filterParam.Column} = {filterParam.Values[0]} ";
                } else
                {
                    query += $" AND {filterParam.Column} in ({string.Join(',', filterParam.Values)})";
                    countQuery += $" AND {filterParam.Column} in ({string.Join(',', filterParam.Values)})";
                }
            }
            if (sortParam != null)
            {
                query += $" ORDER BY {sortParam.Column} " + (sortParam.Desc ? "DESC " : " ");   
            }

            var totalEntryCount = _context.CountByRawSql(countQuery);
            
            if (rangeParam != null)
            {
                query += $" OFFSET {rangeParam.Start} ROWS FETCH NEXT {rangeParam.End - rangeParam.Start + 1} ROWS ONLY;";
                
            }
            else
            {
                query += ";";
                
            }
            
            var topicList = await _context.QuestionTopics.FromSqlRaw(query).AsNoTracking().ToListAsync();
            var count = topicList.Count;

            Response.Headers.Add("Content-Range",
                rangeParam != null
                    ? $"QuestionTopics {rangeParam.Start}-{rangeParam.Start + count - 1}/{totalEntryCount}"
                    : $"QuestionTopics {totalEntryCount}/{totalEntryCount}");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            return topicList;
        }

        

        // GET: api/QuestionTopics/5
        [HttpGet("{id}")]
        public async Task<ActionResult<QuestionTopic>> GetQuestionTopic(int id)
        {
            var questionTopic = await _context.QuestionTopics.FindAsync(id);

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
        [HttpPost("{id}")]
        public async Task<ActionResult<QuestionTopic>> PostQuestionTopic(int id, QuestionTopic questionTopic)
        {
            if (id != questionTopic.Id)
            {
                return BadRequest();
            }
            _context.QuestionTopics.Add(questionTopic);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestionTopic", new { id = questionTopic.Id }, questionTopic);
        }

        // DELETE: api/QuestionTopics/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<QuestionTopic>> DeleteQuestionTopic(int id)
        {
            var questionTopic = await _context.QuestionTopics.FindAsync(id);
            if (questionTopic == null)
            {
                return NotFound();
            }

            _context.QuestionTopics.Remove(questionTopic);
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
                .TakeWhile(q => q.QuestionTopicId == topicId && !q.Deleted)
                .OrderBy(q => q.Rank)
                .Take(number)
                .ToListAsync();
        }


        private bool QuestionTopicExists(int id)
        {
            var topic = _context.QuestionTopics.Find(id);
            if (topic == null || topic.Deleted) {
                return false;
            }
            return true;
        }
    }
}
