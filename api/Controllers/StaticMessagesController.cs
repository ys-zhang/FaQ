using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Controllers.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;


namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    [EnableCors("Debug")]
    public class StaticMessagesController : ControllerBase
    {
        private readonly FaqChatBotDbContext _context;
        private readonly ILogger<StaticMessagesController> _logger;

        private IQueryable<Message> Messages => _context.Messages
            .Include(msg => msg.ContentRelations)
            .ThenInclude(r => r.MessageContent)
            .Include(msg => msg.OptionRelations)
            .ThenInclude(r => r.MessageOption); 

        public StaticMessagesController(FaqChatBotDbContext context, ILogger<StaticMessagesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Message>>> GetMessages(
            [FromQuery] string sort, [FromQuery] string range, [FromQuery] string filter
        )
        {
            var sortParam = SortParam.ParseParam(sort);
            var rangeParam = RangeParam.ParseParam(range);
            var filterParam = FilterParam.ParseParam(filter);
            var query = Messages.Filter(filterParam);
            var totalCount = await query.CountAsync();
            query = query.OrderBy(sortParam).Range(rangeParam);
            var rst = await query.AsNoTracking().ToListAsync();
            Response.Headers.AddContentRange("StaticMessages", rangeParam, totalCount, rst.Count);
            Response.Headers.Add("Access-Control-Expose-Headers", "Content-Range");
            return rst;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Message>> GetMessage(int id)
        {
            
            var msg = _context.Messages.Find(id);
            if (msg == null) return NotFound();
            msg.ContentRelations = await _context.MessageAndContentRelations
                .Where(r => r.MessageId == id)
                .Include(r => r.MessageContent)
                .ToListAsync();
            msg.OptionRelations = await _context.MessageAndOptionRelations
                .Where(r => r.MessageId == id)
                .Include(r => r.MessageOption)
                .ToListAsync();
            return msg;
        }
        
        // TODO wait to be tested
        [HttpPut("{id}")]
        public async Task<ActionResult<Message>> PutMessage(int id, Message message)
        {
            if (message.Id != id) return BadRequest();
            // _logger.LogInformation(message.Options.ToString());
            _logger.LogInformation($"OptionCount: {message.Options.Count}");
            _logger.LogInformation($"ContentCount: {message.Contents.Count}");
            var optionRelations = await _context.MessageAndOptionRelations
                //.Include(r => r.MessageOption)
                .Where(r => r.MessageId == id)
                // .OrderBy(r => r.MessageOptionId)
                .ToListAsync();
            var contentRelations = await _context.MessageAndContentRelations
                //.Include(r => r.MessageContent)
                .Where(r => r.MessageId == id)
                // .OrderBy(r => r.MessageContentId)
                .ToListAsync();
            
            // insert new options and contents
            foreach (var option in message.Options.Where(o => o.Id == 0))
            {
                _context.MessageOptions.Add(option);
            }
            foreach (var content in message.Contents.Where(c => c.Id == 0))
            {
                _context.MessageContents.Add(content);
            }
            await _context.SaveChangesAsync();
            
            
            var optionDict = optionRelations.ToDictionary(r => r.MessageOptionId);
            var contentDict = contentRelations.ToDictionary(r => r.MessageContentId);
            var optionCache = new Dictionary<int, MessageOption>();
            var contentCache = new Dictionary<int, MessageContent>();
            
            // update Options related to this message
            foreach (var r in message.OptionRelations)
            {
                if (r.MessageId != id) return BadRequest();
                if (r.MessageOptionId == 0)
                {
                    r.MessageOptionId = r.MessageOption.Id;
                }
                _logger.LogInformation($"Option in relation: {r.MessageOption.Id}");
                if (optionDict.ContainsKey(r.MessageOptionId))
                {
                    // update old option should through PUT /StaticMessages/Options/{id}
                    optionDict.Remove(r.MessageOptionId);  // what remains in this dict should removed from the db
                }
                else
                {
                    // create new relation
                    optionCache[r.MessageOptionId] = r.MessageOption;
                    r.MessageOption = null; // prevent entity framework insert entities already exists
                    r.Message = null;
                    _context.MessageAndOptionRelations.Add(r);
                }
            }
            
            // update contents related to this message
            foreach (var r in message.ContentRelations)
            {
                if (r.MessageId != id) return BadRequest();
                if (r.MessageContentId == 0)
                {
                    // content is not created
                    r.MessageContentId = r.MessageContent.Id;
                }
                if (contentDict.ContainsKey(r.MessageContentId))
                {
                    contentDict.Remove(r.MessageContentId);
                }
                else
                {
                    contentCache[r.MessageContentId] = r.MessageContent;
                    r.MessageContent = null; // prevent entity framework insert entities already exists
                    r.Message = null; 
                    _context.MessageAndContentRelations.Add(r);
                }
            }
            
            foreach (var r in optionDict.Values)
            {
                // remove option from this message
                _context.MessageAndOptionRelations.Remove(r);
            }
            
            foreach (var r in contentDict.Values)
            {
                // remove option from this message
                _context.MessageAndContentRelations.Remove(r);
            }

            var oldMessage = await _context.Messages.FindAsync(message.Id);
            oldMessage.AnswerType = message.AnswerType;
            await _context.SaveChangesAsync();
            foreach (var r in message.OptionRelations.Where(r => r.MessageOption == null))
            {
                r.MessageOption = optionCache[r.MessageOptionId];
            }
            foreach (var r in message.ContentRelations.Where(r => r.MessageContent == null))
            {
                r.MessageContent = contentCache[r.MessageContentId];
            }
            
            return message;
        }

        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage(Message message)
        {
            if (await MessageExists(message.Id)) return BadRequest();
            foreach (var r in message.ContentRelations)
            {
                // add new content to context tracking store, waited to be written to db
                if (r.MessageContentId == 0)
                {
                    _context.MessageContents.Add(r.MessageContent);
                }
            }

            foreach (var r in message.OptionRelations)
            {
                // add new option to context tracking store, waited to be written to db
                if (r.MessageOptionId == 0)
                {
                    _context.MessageOptions.Add(r.MessageOption);
                }
            }
            await _context.SaveChangesAsync(); // write new Contents and Options to db
            
            // remove Option and Content objects from relations 
            // to avoid second time insertion of existing Option and Content entities
            var optionCache = new Dictionary<int, MessageOption>();
            var contentCache = new Dictionary<int, MessageContent>();
            foreach (var r in message.OptionRelations)
            {
                if (r.MessageOptionId == 0)
                {
                    r.MessageOptionId = r.MessageOption.Id;
                }
                optionCache[r.MessageOptionId] = r.MessageOption;
                r.MessageOption = null;
            }
            foreach (var r in message.ContentRelations)
            {
                if (r.MessageContentId == 0)
                {
                    r.MessageContentId = r.MessageContent.Id;
                }
                contentCache[r.MessageContentId] = r.MessageContent;
                r.MessageContent = null;
            }
            
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();  // write message to db
            
            // restore Content and Option objects in relations
            // for return, if not old Contents and Options will be null 
            // in the returned Json.
            foreach (var r in message.ContentRelations)
            {
                r.MessageId = message.Id;
                if (r.MessageContent == null)
                {
                    r.MessageContent = contentCache[r.MessageContentId];
                }
            }

            foreach (var r in message.OptionRelations)
            {
                r.MessageId = message.Id;
                if (r.MessageOption == null)
                {
                    r.MessageOption = optionCache[r.MessageOptionId];
                }
            }
            
            return CreatedAtAction("GetMessage", new { Id = message.Id }, message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Message>> DeleteMessage(int id)
        {
            var message = await Messages.FirstAsync(m => m.Id == id);
            if (message == null) return NotFound();
            _context.Messages.Remove(message);
            await _context.SaveChangesAsync();
            return message;
        }
        
        
        //  =======================  Message Options ===============================
        [HttpGet("options")]
        public async Task<ActionResult<IEnumerable<MessageOption>>> GetOptions(
            [FromQuery] string filter, [FromQuery] string sort, [FromQuery] string range)
        {
            var filterParam = FilterParam.ParseParam(filter);
            var sortParam = SortParam.ParseParam(sort);
            var rangeParam = RangeParam.ParseParam(range);
            var query = _context.MessageOptions
                .AsQueryable()
                .Filter(filterParam);
            var total = await query.CountAsync();
            var rst = await query.OrderBy(sortParam).Range(rangeParam).ToListAsync();
            Response.Headers.AddContentRange("MessageOptions", rangeParam, total, rst.Count);
            return rst;
        }

        [HttpGet("options/{id}")]
        public async Task<ActionResult<MessageOption>> GetOption(int id)
        {
            var option = await _context.MessageOptions.FindAsync(id);
            if (option == null) return NotFound();
            return option;
        }

        [HttpPost("options")]
        public async Task<ActionResult<MessageOption>> CreateOption(MessageOption option)
        {
            if (option == null || await OptionExists(option.Id))
            {
                return BadRequest();
            }

            _context.MessageOptions.Add(option);
            await _context.SaveChangesAsync();
            return option;
        }

        [HttpPut("options/{id}")]
        public async Task<ActionResult<MessageOption>> UpdateOption(int id, MessageOption option)
        {
            if (id != option.Id) return BadRequest();
            _context.Entry(option).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (! await OptionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return option;
        }

        [HttpDelete("options/{id}")]
        public async Task<ActionResult<MessageOption>> DeleteOption(int id)
        {
            var option = await _context.MessageOptions.FindAsync(id);
            if (option == null) return NotFound();
            _context.MessageOptions.Remove(option);
            await _context.SaveChangesAsync();
            return option;
        }

        private async Task<bool> OptionExists(int id)
        {
            return await _context.MessageOptions.AnyAsync(o => o.Id == id);
        }

        private async Task<bool> MessageExists(int id)
        {
            return await _context.Messages.AnyAsync(m => m.Id == id);
        }
    }
}