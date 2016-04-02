using System;
using System.Collections.Generic;
using ActiveStateMachine.States;

namespace ActiveStateMachine.Messages
{
    public sealed class PossibleStatesReply : StateMachineReply
    {
        public PossibleStatesReply(string source, string target, IEnumerable<State> states) 
            : base(new Version(1,0), "possible states", source, target, string.Empty)
        {
            PossibleStates = states;
        }

        public IEnumerable<State> PossibleStates { get; }
    }
}