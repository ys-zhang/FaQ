using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using api.Models;


namespace api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FAQController : ControllerBase
    {
        private readonly ILogger<FAQController> _logger;
        private readonly FAQChatBotDBContext _dbContext;

        public FAQController(ILogger<FAQController> logger, FAQChatBotDBContext dbContext)
        {
            _logger = logger;
            _dbContext = dbContext;
        }

        [HttpGet("questions/{id:int}")]
        public async Task<ActionResult<Question>> GetQuestion(int id)
        {
            var q = await _dbContext.questions.FindAsync(id);
            if (q == null || q.Deleted)
            {
                return NotFound();
            }
            return q;
        }

        [HttpGet("questions")]
        public async Task<List<Question>> GetQuestions([FromQuery] RangeSelection range)
        {
            IQueryable<Question> query = _dbContext.questions.AsQueryable();
                                  
            return await query.ToListAsync();
        }


        [HttpDelete("questions/{id:int}")]
        public async Task<ActionResult> DeleteQestion(int id)
        {
            var q = await _dbContext.questions.FindAsync(id);
            if (q == null)
            {
                return NotFound($"Qestion ID: {id}");
            }
            else if (q.Deleted)
            {
                return new StatusCodeResult(410); // already deleted
            }
            q.Deleted = true;
            _dbContext.Update(q);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("questions")]
        public async Task<ActionResult> CreateQuestion([FromBody] Question question)
        {
            question.updateTime = DateTime.Now;
            try
            {
                _dbContext.questions.Add(question);
                await _dbContext.SaveChangesAsync();
            } catch (Exception e)
            {
                return Problem(e.Message, e.GetType().ToString(), statusCode: 500);
            }
            return NoContent();
        }

        [HttpPut("questions/{id:int}")]
        public async Task<ActionResult> UpdateQuestion(int id, [FromBody] Question question)
        {
            var q = await _dbContext.questions.FindAsync(id);
            if (q == null || q.Deleted)
            {
                return NotFound($"Qestion ID: {id}");
            }
            question.Id = q.Id;
            question.updateTime = DateTime.Now;
            try
            {
                _dbContext.questions.Update(question);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Problem(e.Message, e.GetType().ToString(), statusCode: 500);
            }
            var stateChange = $"Question updated at {question.updateTime}: \n";
            if (q.Description != question.Description) stateChange += $"\tDescription: {q.Description}=>{question.Description}\n";
            if (q.Content != question.Content) stateChange += $"\tContent: {q.Content}=>{question.Content}\n";
            if (q.Answer != question.Answer) stateChange += $"\tAnswer: {q.Answer}=>{question.Answer}\n";
            if (q.Active != question.Active) stateChange += $"\tActivateState Changed";
            if (q.QuestionTopicId != question.QuestionTopicId)
            {
                // TODO dbcontext set eager load mode
                stateChange += $"\tTopic: {q.questionTopic.Name}=>{question.questionTopic.Name}\n";
            }
            return Content(stateChange);
        }

        [HttpPatch("questions")]
        public async Task<ActionResult> UpdateQuestion([FromBody] Question question)
        {
            if ((await _dbContext.questions.FindAsync(question.Id)) != null)
            {
                return await UpdateQuestion(question.Id, question);
            }
            else
            {
                return await CreateQuestion(question);
            }

        }

        
       
        [HttpGet("questionTopics/{id:int}")]
        public async Task<QuestionTopic> GetQuestionTopic(int id) => await _dbContext.questionTopics.FindAsync(id);

        [HttpDelete("questionTopics/{id:int}")]
        public async Task<ActionResult> DeleteTopic(int id)
        {
            var topic = await _dbContext.questionTopics.FindAsync(id);
            if (topic == null)
            {
                return NotFound($"QestionTopic ID: {id}");
            }
            else if (topic.Deleted)
            {
                return new StatusCodeResult(410); // already deleted
            }
            else if (await _dbContext.questions.AnyAsync(q => q.QuestionTopicId == id && q.Deleted == false))
            {
                return Problem($"topic: {id} contains questions", statusCode: 409);
            }

            topic.Deleted = true;
            _dbContext.Update(topic);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPost("questionTopics")]
        public async Task<ActionResult> CreateQuestionTopic([FromBody] QuestionTopic topic)
        {
            topic.updateTime = DateTime.Now;
            try
            {
                _dbContext.questionTopics.Add(topic);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Problem(e.Message, e.GetType().ToString(), statusCode: 500);
            }
            return NoContent();
        }

        [HttpPatch("questionTopics")]
        public async Task<ActionResult> UpdateQuestionTopic([FromBody] QuestionTopic topic)
        {
            if ((await _dbContext.questionTopics.FindAsync(topic.Id)) != null)
            {
                return await UpdateQuestionTopic(topic.Id, topic);
            } else
            {
                return await CreateQuestionTopic(topic);
            }
                
        }

        [HttpPut("questionTopics/{id:int}")]
        public async Task<ActionResult> UpdateQuestionTopic(int id, [FromBody] QuestionTopic topic)
        {
            var t = await _dbContext.questionTopics.FindAsync(id);
            if (t == null || t.Deleted)
            {
                return NotFound($"Qestion ID: {id}");
            }
            topic.Id = t.Id;
            topic.updateTime = DateTime.Now;
            try
            {
                _dbContext.questionTopics.Update(topic);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Problem(e.Message, e.GetType().ToString(), statusCode: 500);
            }
            var stateChange = $"Question updated at {topic.updateTime}: \n";
            if (t.Name != topic.Name) stateChange += $"\tName: {t.Name}=>{topic.Name}\n";
            if (t.Icon != topic.Icon) stateChange += $"\tIcon changed";
            if (t.Active != topic.Active) stateChange += $"\tActivateState Changed";
            return Content(stateChange);
        }

        

        [HttpGet]
        public int Index() => 1;
    }
}
