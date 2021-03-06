using System.Collections.Generic;

namespace ActiveStateMachine.Transitions
{
    public sealed class Transition
    {
        public string Name { get; }
        public string SourceStateName { get; }
        public string TargetStateName { get; }

        /// <summary>
        /// List of checks that need to be done before the transition.
        /// </summary>
        public IEnumerable<TransitionPrecondition> Preconditions { get; }

        /// <summary>
        /// Actions to execute during the transition
        /// </summary>
        public IEnumerable<TransitionAction> TransitionActions { get; }

        public Transition(string name, string sourceStateName, string targetStateName, 
            IEnumerable<TransitionPrecondition> preconditions, IEnumerable<TransitionAction> transitionActions)
        {
            Name = name;
            SourceStateName = sourceStateName;
            TargetStateName = targetStateName;
            Preconditions = preconditions;
            TransitionActions = transitionActions;
        }
    }
}