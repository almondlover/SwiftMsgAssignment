using Microsoft.Data.Sqlite;
using SwiftMsgAssignment.Controllers;
using SwiftMsgAssignment.Models;

namespace SwiftMsgAssignment.Services
{
    public class MessageService : IMessageService
    {
        private readonly ILogger<MessageService> _logger;
        public MessageService(ILogger<MessageService> logger)
        {
            _logger = logger;
        }
        public SwiftMT799 ParseMessage(string msg)
        {
            string[] blockContent = new string[5];

            //first block
            var basicHeaderBlock = new BasicHeaderBlock();
            blockContent[0] = msg.SubstringInbetween("{1:", "}");
            //block is mandatory and its fields are all mandatory so its mandatory length is constant
            if (blockContent[0] == null || blockContent[0].Length != 25) return null;
            //extract fields
            basicHeaderBlock.AppID = blockContent[0][0];
            basicHeaderBlock.ServiceID = blockContent[0].Substring(1, 2);
            basicHeaderBlock.LTAddress = blockContent[0].Substring(3, 12);
            basicHeaderBlock.SessionNumber = blockContent[0].Substring(15, 4);
            basicHeaderBlock.SequenceNumber = blockContent[0].Substring(19, 6);

            //second block
            var applicationHeaderBlock = new ApplicationHeaderBlock();
            blockContent[1] = msg.SubstringInbetween("{2:", "}");
            //mandatory elements have a total length of 46
            if (blockContent[1].Length < 46 || blockContent[1].Length > 47) return null;
            //extract fields
            applicationHeaderBlock.IOID = blockContent[1][0];
            applicationHeaderBlock.SWIFTMT = int.Parse(blockContent[1].Substring(1, 3));
            applicationHeaderBlock.InputTime = TimeOnly.ParseExact(blockContent[1].Substring(4, 4).Insert(2, ":"), "hh:mm");
            applicationHeaderBlock.MIR = blockContent[1].Substring(8, 28);
            applicationHeaderBlock.OutputDate = DateOnly.ParseExact(blockContent[1].Substring(36, 6).Insert(2, " ").Insert(5, " "), "yy mm dd");
            applicationHeaderBlock.OutputTime = TimeOnly.ParseExact(blockContent[1].Substring(42, 4).Insert(2, ":"), "hh:mm");
            applicationHeaderBlock.Priority = blockContent[1].Length > 16 ? blockContent[1][46] : null;

            //third block
            blockContent[2] = msg.SubstringInbetween("{3:{", "}}");
            var userHeaderBlock = blockContent[2]?.Split("}{");

            //fourth block
            var mt799TextBlock = new MT799TextBlock();

            blockContent[3] = msg.SubstringInbetween("{4:", "}");
            if (blockContent[3] != null)
            {
                //extracts the fields
                mt799TextBlock.TransactionReference = blockContent[3].SubstringInbetween(":20:", ":");
                //mandatory field
                if (mt799TextBlock.TransactionReference == null) return null;
                mt799TextBlock.TransactionReference = mt799TextBlock.TransactionReference.Replace("\r\n", "").Trim(' ');

                mt799TextBlock.RelatedReference = blockContent[3].SubstringInbetween(":21:", ":");
                mt799TextBlock.RelatedReference = mt799TextBlock.RelatedReference?.Replace("\r\n", "").Trim(' ');

                mt799TextBlock.Narrative = blockContent[3].SubstringInbetween(":79:", "-");
                //mandatory field
                if (mt799TextBlock.Narrative == null) return null;
                //maybe remove newlines for db entries?
            }

            //fifth block
            blockContent[4] = msg.SubstringInbetween("{5:{", "}}");
            var trailersBlock = blockContent[4]?.Split("}{");

            //build the full msg model
            var smt799 = new SwiftMT799();
            smt799.BasicHeader = basicHeaderBlock;
            smt799.ApplicationHeader = applicationHeaderBlock;
            smt799.UserHeader = userHeaderBlock;
            smt799.Text = mt799TextBlock;
            smt799.Trailers = trailersBlock;

            return smt799;
        }

        public void UploadFields(SwiftMT799 smt799)
        {
            //save fields to db
            using (var sqliteConn = new SqliteConnection("Data Source=swift-message-management.db"))
            {
                sqliteConn.Open();

                var cmd = sqliteConn.CreateCommand();

                cmd.CommandText =
                    @"
                        INSERT INTO messages (transaction_reference_number, related_reference, narrative)
                        VALUES ($tr, $rr, $n)
                    ";
                cmd.Parameters.AddWithValue("$tr", smt799.Text.TransactionReference);
                cmd.Parameters.AddWithValue("$rr", smt799.Text.RelatedReference);
                cmd.Parameters.AddWithValue("$n", smt799.Text.Narrative);

                cmd.ExecuteNonQuery();

            }
        }
    }
}
