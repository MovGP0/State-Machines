using System;

namespace ActiveStateMachine.Messages
{
    public sealed class GetPossibleStatesRequest : StateMachineReqest
    {
        public GetPossibleStatesRequest(string source, string target, string messageInfo) 
            : base(new Version(1,0), "get possible states", source, target, messageInfo)
        {
        }
    }
}