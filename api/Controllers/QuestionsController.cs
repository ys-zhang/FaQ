using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using api.Models;
using Microsoft.AspNetCore.Cors;
using api.Controllers.Params;

namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    [EnableCors("Debug")]
    public class QuestionsController : ControllerBase
    {
        private readonly FaqChatBotDbContext _context;

        public QuestionsController(FaqChatBotDbContext context)
        {
            _context = context;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> GetQuestions(
            [FromQuery] string sort, [FromQuery] string range, [FromQuery] string filter)
        {
            var sortParam = SortParam.ParseParam(sort) ;
            var rangeParam = RangeParam.ParseParam(range);
            var filterParam = FilterParam.ParseParam(filter);
            var query = "SELECT * FROM Questions WHERE Deleted = 0";
            var countQuery = "SELECT count(*) FROM Questions WHERE Deleted = 0";
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
            
            var topicList = await _context.Questions.FromSqlRaw(query).AsNoTracking().ToListAsync();
            var count = topicList.Count;

            Response.Headers.Add("Content-Range",
                rangeParam != null
                    ? $"Questions {rangeParam.Start}-{rangeParam.Start + count - 1}/{totalEntryCount}"
                    : $"Questions {totalEntryCount}/{totalEntryCount}");
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            // Response.Headers.Add("Access-Control-Allow-Origin", "*");
            return topicList;
        }

        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);

            if (question == null)
            {
                return NotFound();
            }

            return question;
        }

        // PUT: api/Questions/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuestion(int id, Question question)
        {
            if (id != question.Id)
            {
                return BadRequest();
            }

            _context.Entry(question).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuestionExists(id))
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

        // POST: api/Questions
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        [HttpPost]
        public async Task<ActionResult<Question>> PostQuestion(Question question)
        {
            // TODO
            //if (question.Id != 0 && QuestionExists(question.Id))
            //{
            //    return BadRequest();
            //}
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestion", new { id = question.Id }, question);
        }

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Question>> DeleteQuestion(int id)
        {
            var question = await _context.Questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.Questions.Remove(question);
            await _context.SaveChangesAsync();

            return question;
        }

        [HttpGet("top/{number:int}")]
        public async Task<List<Question>> TopQuestions(int number) 
            => await _context.Questions
            .TakeWhile(t => !t.Deleted)
            .OrderBy(q => q.Rank)
            .Take(number)
            .ToListAsync();

        private bool QuestionExists(int id)
        {
            return _context.Questions.Any(e => e.Id == id);
        }
    }
}
