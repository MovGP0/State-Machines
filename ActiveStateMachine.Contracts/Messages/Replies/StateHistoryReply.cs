using System;
using System.Collections.Generic;
using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages
{
    public sealed class StateHistoryReply : StateMachineReply
    {
        public StateHistoryReply(string source, string target, IEnumerable<State> states) 
            : base(new Version(1,0), "state history", source, target, string.Empty)
        {
            StateHistory = states;
        }

        public IEnumerable<State> StateHistory { get; }
    }
}