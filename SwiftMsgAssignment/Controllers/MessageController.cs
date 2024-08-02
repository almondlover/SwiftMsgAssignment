using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using SwiftMsgAssignment.Models;
using System.Xml.Linq;

namespace SwiftMsgAssignment.Controllers
{
    public enum SwiftMessageBlock
    { 
        BasicHeader=1,
        ApplicationHeader,
        UserHeader,
        Text,
        Trailer
    }
    public enum MT799Fields
    { 
        TransactionReference,
        RelatedReference,
        Narrative
    }
    [Route("mt799api/[controller]/")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        public MessageController()
        {

        }
        [HttpPost("ParseSwiftMT799")]
        public IActionResult ReadMessage(IFormFile file)
        {
            var mt799TextBlock = new MT799TextBlock();
            string msg = "";

            using (var sreader = new StreamReader(file.OpenReadStream()))
                msg=sreader.ReadToEnd();

            mt799TextBlock = ParseMessage(msg);
            if (mt799TextBlock == null) return BadRequest();

            using (var sqliteConn=new SqliteConnection("Data Source=swift-message-management.db"))
            {
                sqliteConn.Open();

                var cmd = sqliteConn.CreateCommand();

                cmd.CommandText =
                    @"
                        INSERT INTO messages (transaction_reference_number, related_reference, narrative)
                        VALUES ($tr, $rr, $n)
                    ";
                cmd.Parameters.AddWithValue("$tr", mt799TextBlock.TransactionReference);
                cmd.Parameters.AddWithValue("$rr", mt799TextBlock.RelatedReference);
                cmd.Parameters.AddWithValue("$n", mt799TextBlock.Narrative);

                cmd.ExecuteNonQuery();

            }
            return Ok(mt799TextBlock);
        }
        private MT799TextBlock ParseMessage(string msg)
        {
            var mt799TextBlock = new MT799TextBlock();

            string textBlockContent = msg.SubstringInbetween("{4:", "}");

            //extracts the fields
            mt799TextBlock.TransactionReference = textBlockContent.SubstringInbetween(":20:", ":");
            //mandatory field
            if (mt799TextBlock.TransactionReference == null) return null;
            mt799TextBlock.TransactionReference = mt799TextBlock.TransactionReference.Replace("\r\n", "").Trim(' ');

            mt799TextBlock.RelatedReference = textBlockContent.SubstringInbetween(":21:", ":");
            mt799TextBlock.RelatedReference = mt799TextBlock.RelatedReference?.Replace("\r\n", "").Trim(' ');

            mt799TextBlock.Narrative = textBlockContent.SubstringInbetween(":79:", "-");
            //mandatory field
            if (mt799TextBlock.Narrative == null) return null;
            //maybe remove newlines for db entries?

            return mt799TextBlock;
        }
    }
}
