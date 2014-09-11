// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor
{
    public abstract class StateMachine<TReturn>
    {
        protected delegate StateResult State();

        protected abstract State StartState { get; }

        protected State CurrentState { get; set; }

        protected virtual TReturn Turn()
        {
            if (CurrentState != null)
            {
                StateResult result;
                do
                {
                    // Keep running until we get a null result or output
                    result = CurrentState();
                    CurrentState = result.Next;
                }
                while (!result.HasOutput);

                return result.Output;
            }
            return default(TReturn);
        }

        /// <summary>
        /// Returns a result indicating that the machine should stop executing and return null output.
        /// </summary>
        protected StateResult Stop()
        {
            return new StateResult();
        }

        /// <summary>
        /// Returns a result indicating that this state has no output and the machine should immediately invoke the specified state
        /// </summary>
        /// <remarks>
        /// By returning no output, the state machine will invoke the next state immediately, before returning
        /// controller to the caller of <see cref="Turn"/>
        /// </remarks>
        protected StateResult Transition(State newState)
        {
            return new StateResult(newState);
        }

        /// <summary>
        /// Returns a result containing the specified output and indicating that the next call to
        /// <see cref="Turn"/> should invoke the provided state.
        /// </summary>
        protected StateResult Transition(TReturn output, State newState)
        {
            return new StateResult(output, newState);
        }

        /// <summary>
        /// Returns a result indicating that this state has no output and the machine should remain in this state
        /// </summary>
        /// <remarks>
        /// By returning no output, the state machine will re-invoke the current state again before returning
        /// controller to the caller of <see cref="Turn"/>
        /// </remarks>
        protected StateResult Stay()
        {
            return new StateResult(CurrentState);
        }

        /// <summary>
        /// Returns a result containing the specified output and indicating that the next call to
        /// <see cref="Turn"/> should re-invoke the current state.
        /// </summary>
        protected StateResult Stay(TReturn output)
        {
            return new StateResult(output, CurrentState);
        }

        protected struct StateResult
        {
            private readonly bool _hasOutput = true;
            private readonly TReturn _output;
            private readonly State _next;

            public StateResult(State next)
            {
                _next = next;

                _hasOutput = false;
                _output = default(TReturn);
            }

            // Emit a result with a follow up state
            public StateResult(TReturn output, State next)
            { 
                _output = output;
                _next = next;

                _hasOutput = true;
            }

            public bool HasOutput { get { return _hasOutput; } }
            public TReturn Output { get { return _output; } }
            public State Next { get { return _next; } }
        }
    }
}
