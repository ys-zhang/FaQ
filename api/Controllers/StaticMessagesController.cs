using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        [HttpPut("id")]
        public async Task<ActionResult<Message>> PutMessage(int id, Message message)
        {
            if (message.Id != id) return BadRequest();
            var optionRelations = await _context.MessageAndOptionRelations
                .Include(r => r.MessageOption)
                .Where(r => r.MessageId == id)
                .OrderBy(r => r.MessageOptionId)
                .ToListAsync();
            var contentRelations = await _context.MessageAndContentRelations
                .Include(r => r.MessageContent)
                .Where(r => r.MessageId == id)
                .OrderBy(r => r.MessageContentId)
                .ToListAsync();
            
            // update Options related to this message
            var optionDict = optionRelations.ToDictionary(r => r.MessageOptionId);
            foreach (var r in message.OptionRelations)
            {
                if (r.MessageId != id) return BadRequest();
                if (r.MessageOptionId == 0)
                {
                    // option is not created
                    if (r.MessageOption == null || r.MessageOption.Id > 0) return BadRequest("Expecting an option ...");
                    _context.MessageOptions.Add(r.MessageOption);
                    await _context.SaveChangesAsync();
                    if (r.MessageOption.Id == 0)
                    {
                        _logger.LogError("New option created but id is not updated");
                        return BadRequest();
                    }
                    r.MessageOptionId = r.MessageOption.Id;
                }

                if (!optionDict.ContainsKey(r.MessageOptionId))
                {
                    // create new relation
                    _context.MessageAndOptionRelations.Add(r);
                }
                else
                {
                    var old = optionDict[r.MessageOptionId].MessageOption;
                    if (!old.Equals(r.MessageOption))
                    {
                        // Option changed
                        _context.Entry(r.MessageOption).State = EntityState.Modified;
                    }

                    optionDict.Remove(r.MessageOptionId);
                }
            }

            foreach (var r in optionDict.Values)
            {
                // remove option from this message
                _context.MessageAndOptionRelations.Remove(r);
            }
            
            // update contents related to this message
            var contentDict = contentRelations.ToDictionary(r => r.MessageContentId);
            foreach (var r in message.ContentRelations)
            {
                if (r.MessageId != id) return BadRequest();
                if (r.MessageContentId == 0)
                {
                    // content is not created
                    if (r.MessageContent == null || r.MessageContent.Id > 0) return BadRequest("Expecting an content ...");
                    _context.MessageContents.Add(r.MessageContent);
                    await _context.SaveChangesAsync();
                    if (r.MessageContent.Id == 0)
                    {
                        _logger.LogError("New content created but id is not updated");
                        return BadRequest();
                    }
                    r.MessageContentId = r.MessageContent.Id;
                }

                if (!optionDict.ContainsKey(r.MessageContentId))
                {
                    // create new relation
                    _context.MessageAndContentRelations.Add(r);
                }
                else
                {
                    var old = contentDict[r.MessageContentId].MessageContent;
                    if (!old.Equals(r.MessageContent))
                    {
                        // Option changed
                        _context.Entry(r.MessageContent).State = EntityState.Modified;
                    }

                    optionDict.Remove(r.MessageContentId);
                }
            }

            foreach (var r in contentDict.Values)
            {
                // remove option from this message
                _context.MessageAndContentRelations.Remove(r);
            }
            

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
    }
}