﻿using System.Collections;
using System.Collections.Generic;
using UniModule.UnityTools.ActorEntityModel;
using UniModule.UnityTools.Common;
using UniModule.UnityTools.DataFlow;
using UniModule.UnityTools.Extension;
using UniModule.UnityTools.Interfaces;
using UniModule.UnityTools.UniPool.Scripts;
using UniModule.UnityTools.ProfilerTools;
using UniModule.UnityTools.UniStateMachine.Interfaces;

namespace UniModule.UnityTools.UniStateMachine.ContextStateMachine
{
    public enum StateStatus : byte
    {
        Initialization,
        Execution,
        Complete,
        Disable,
    }
    
    public abstract class ContextState : IContextState<IEnumerator>
    {
        /// <summary>
        /// current state status
        /// </summary>
        private StateStatus _stateStatus = StateStatus.Disable;
        
        /// <summary>
        /// state lifeTime context
        /// </summary>
        protected LifeTimeDefinition _lifeTime = new LifeTimeDefinition();

        /// <summary>
        /// state local context data
        /// </summary>
        protected EntityObject _contextData = new EntityObject();

        
        #region public properties

        public ILifeTime LifeTime => _lifeTime.LifeTime;

        public bool IsActive => _stateStatus != StateStatus.Disable;
        
        #endregion
        
        #region public methods

        public IEnumerator Execute(IContext context)
        {
            //if state already active - wait
            if (IsActive)
            {
                yield return this.WaitWhile(() => IsActive);
                yield break;
            }

            SetStatus(StateStatus.Initialization);
            
            Initialize(_contextData);

            SetStatus(StateStatus.Execution);
            
            yield return ExecuteState(context);

            OnPostExecute(context);
            
            SetStatus(StateStatus.Complete);
        }

        public void Exit()
        {
            if(!IsActive)
                return;
            
            SetStatus(StateStatus.Disable);
            
            OnExit(_contextData);

            _lifeTime.Terminate();
        }
        
        public void Release()
        {
            Exit();
        }

        #endregion

        protected void Initialize(IContext context)
        {
            LifeTime.AddCleanUpAction(() => this._contextData.Release());
            LifeTime.AddCleanUpAction(() => this._stateStatus = StateStatus.Disable);
            
            OnInitialize(_contextData);
        }
        
        protected virtual void OnInitialize(IContext stateContext) { }

        protected virtual void OnExit(IContext context){}

        protected virtual void OnPostExecute(IContext context){}

        protected abstract IEnumerator ExecuteState(IContext context);

        private void SetStatus(StateStatus status)
        {
            _stateStatus = status;
        }
        
    }
}
