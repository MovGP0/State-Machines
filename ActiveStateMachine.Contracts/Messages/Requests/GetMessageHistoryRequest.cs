using System;

namespace ActiveStateMachine.Messages
{
    public sealed class GetMessageHistoryRequest : StateMachineReqest
    {
        public GetMessageHistoryRequest(string source, string target, string messageInfo) 
            : base(new Version(1,0), "get message history", source, target, messageInfo)
        {
        }
    }
}