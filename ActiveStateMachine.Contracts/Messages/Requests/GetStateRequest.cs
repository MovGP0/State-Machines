using System;

namespace ActiveStateMachine.Messages
{
    public sealed class GetStateRequest : StateMachineReqest
    {
        public GetStateRequest(string source, string target, string messageInfo) 
            : base(new Version(1,0), "get state", source, target, messageInfo)
        {
        }
    }
}