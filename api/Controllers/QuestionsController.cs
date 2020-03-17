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
    public class QuestionsController : ControllerBase
    {
        private readonly FAQChatBotDBContext _context;

        public QuestionsController(FAQChatBotDBContext context)
        {
            _context = context;
        }

        // GET: api/Questions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Question>>> Getquestions()
        {
            return await _context.questions.ToListAsync();
        }

        // GET: api/Questions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var question = await _context.questions.FindAsync(id);

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
        [HttpPost("{id}")]
        public async Task<ActionResult<Question>> PostQuestion(int id, Question question)
        {
            question.Id = id;
            _context.questions.Add(question);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuestion", new { id = question.Id }, question);
        }

        // DELETE: api/Questions/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Question>> DeleteQuestion(int id)
        {
            var question = await _context.questions.FindAsync(id);
            if (question == null)
            {
                return NotFound();
            }

            _context.questions.Remove(question);
            await _context.SaveChangesAsync();

            return question;
        }

        [HttpGet("top/{number:int}")]
        public async Task<List<Question>> TopQuestions(int number) 
            => await _context.questions
            .TakeWhile(t => !t.Deleted)
            .OrderBy(q => q.Rank)
            .Take(number)
            .ToListAsync();

        private bool QuestionExists(int id)
        {
            return _context.questions.Any(e => e.Id == id);
        }
    }
}
