using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Controllers.AuthUtil;
using Microsoft.AspNetCore.Mvc;
using api.Models;
using api.Controllers.Params;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic;


namespace api.Controllers
{
    [Route("faq/[controller]")]
    [ApiController]
    [EnableCors("Debug")]
    public class StaticMessagesController : ControllerBase
    {
        private readonly FaqChatBotDbContext _context;
        private readonly ILogger<StaticMessagesController> _logger;
        private readonly JwtDecoder _jwtDecoder;

        private IQueryable<Message> Messages => _context.Messages
            .Include(msg => msg.MessageContents)
            .Include(msg => msg.MessageOptions);

        public StaticMessagesController(FaqChatBotDbContext context, ILogger<StaticMessagesController> logger, JwtDecoder jwtDecoder)
        {
            _context = context;
            _logger = logger;
            _jwtDecoder = jwtDecoder;
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
            await _context.Entry<Message>(msg).Collection(m => m.MessageContents).LoadAsync();
            await _context.Entry<Message>(msg).Collection(m => m.MessageOptions).LoadAsync();
            return msg;
        }
        
        [HttpPut("{id}")]
        public async Task<ActionResult<Message>> PutMessage(int id, Message message)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
            if (message.Id != id) return BadRequest();
            _context.Messages.Update(message);
            await _context.SaveChangesAsync();
            return message;
        }

        [HttpPost]
        public async Task<ActionResult<Message>> CreateMessage(Message message)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
            if (await MessageExists(message.Id)) return BadRequest();
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetMessage", new { Id = message.Id }, message);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<Message>> DeleteMessage(int id)
        {
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
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
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
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
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
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
            if (!AuthenticateEditor()) return Unauthorized("Sorry you don't have the permission to create or edit is resource");
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
        
        private bool AuthenticateEditor()
        {
            if (!_jwtDecoder.Verify(Request)) return false;
            var payload = JwtDecoder.ParsePayload(Request);
            if (payload.Expiration < DateTime.Now) return false;
            if (payload.Roles.Contains(AdminUserRole.Admin)) return true;
            return false;
        }
    }
}