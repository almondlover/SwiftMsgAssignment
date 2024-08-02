using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SwiftMsgAssignment.Models;
using SwiftMsgAssignment.Services;
using System.Xml.Linq;

namespace SwiftMsgAssignment.Controllers
{
    [Route("mt799api/[controller]/")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IMessageService _messageService;
        public MessageController(ILogger<MessageController> logger, IConfiguration configuration, IMessageService messageService)
        {
            _messageService = messageService;
        }
        [HttpPost("ParseSwiftMT799")]
        public IActionResult ReadMessage(IFormFile file)
        {
            var smt799 = new SwiftMT799();
            string msg = "";

            using (var sreader = new StreamReader(file.OpenReadStream()))
                msg=sreader.ReadToEnd();

            smt799 = _messageService.ParseMessage(msg);
            if (smt799 == null) return BadRequest();

            _messageService.UploadFields(smt799);
            
            return Ok(smt799);
        }
    }
}
