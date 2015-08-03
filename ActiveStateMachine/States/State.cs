using System.Collections.Generic;
using ActiveStateMachine.Transitions;

namespace ActiveStateMachine.States
{
    public abstract class State
    {
        protected State(string stateName,
            IEnumerable<Transition> stateTransitionList,
            IEnumerable<TransitionAction> entryActions,
            IEnumerable<TransitionAction> exitActions,
            bool isDefaultState)
        {
            StateName = stateName;
            StateTransitionList = stateTransitionList;
            EntryActions = entryActions;
            ExitActions = exitActions;
            IsDefaultState = isDefaultState;
        }

        public string StateName { get; }
        public IEnumerable<Transition> StateTransitionList { get; }
        public IEnumerable<TransitionAction> EntryActions { get; }
        public IEnumerable<TransitionAction> ExitActions { get; }
        public bool IsDefaultState { get; }
    }
}