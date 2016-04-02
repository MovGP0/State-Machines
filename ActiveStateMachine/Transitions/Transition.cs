using System;
using System.Collections.Generic;
using System.Linq;
using Anotar.NLog;
using MoreLinq;
using NLog.Fluent;

namespace ActiveStateMachine.Transitions
{
    public sealed class Transition : IObserver<string>
    {
        public string Name { get; }
        public string SourceStateName { get; }
        public string TargetStateName { get; }
        private bool IsCompleted { get; set; }

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

        [LogToErrorOnException]
        public void OnNext(string value)
        {
            if (IsCompleted) return;

            if (Preconditions.All(p => p.IsValid))
            {
                TransitionActions.ForEach(t => t.Execute());
            }
        }

        public void OnError(Exception error)
        {
            if (IsCompleted) return;

            Log.Trace()
                .Message($"Handling error in transition {Name}.")
                .Exception(error);

            OnCompleted();
        }
        
        [LogToErrorOnException]
        public void OnCompleted()
        {
            IsCompleted = true;
        }
    }
}