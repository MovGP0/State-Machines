using System;

namespace ActiveStateMachine.Transitions
{
    public sealed class TransitionAction
    {
        public string Name { get; }
        private Action Method { get; }

        public TransitionAction(string name, Action method)
        {
            Name = name;
            Method = method;
        }

        public void Execute()
        {
            Method();
        }
    }
}
