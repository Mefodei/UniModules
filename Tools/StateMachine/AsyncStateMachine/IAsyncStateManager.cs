﻿namespace Assets.Scripts.Tools.StateMachine.AsyncStateMachine
{
    public interface IAsyncStateManager<TStateType>
    {
        TStateType CurrentState { get; }
        TStateType PreviousState { get; }

        IStateController<TStateType> StateController { get; }

        void Dispose();
        void SetState(TStateType state);
    }
}