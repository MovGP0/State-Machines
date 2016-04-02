using System;

namespace ActiveStateMachine.Messages
{
    public sealed class GetStateHistoryRequest : StateMachineReqest
    {
        public GetStateHistoryRequest(string source, string target, string messageInfo) 
            : base(new Version(1,0), "get state history", source, target, messageInfo)
        {
        }
    }
}