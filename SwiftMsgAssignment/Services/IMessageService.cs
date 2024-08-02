using SwiftMsgAssignment.Models;

namespace SwiftMsgAssignment.Services
{
    public interface IMessageService
    {
        SwiftMT799 ParseMessage(string msg);
        void UploadFields(SwiftMT799 smt799);
    }
}
