using System;

namespace ActiveStateMachine.Transitions
{
    public sealed class TransitionPrecondition
    {
        public string Name { get; }
        private Func<bool> CheckPrecondition { get; }

        public TransitionPrecondition(string name, Func<bool> precondition)
        {
            Name = name;
            CheckPrecondition = precondition;
        }

        public bool IsValid => CheckPrecondition();
    }
}