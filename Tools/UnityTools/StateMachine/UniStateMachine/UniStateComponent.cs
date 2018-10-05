﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Tools.UnityTools.Extension;
using Assets.Tools.UnityTools.Interfaces;
using Assets.Tools.UnityTools.StateMachine.Interfaces;
using UnityEngine;

namespace Assets.Tools.UnityTools.StateMachine.UniStateMachine {
    
    public class UniStateComponent : MonoBehaviour,IContextStateBehaviour<IEnumerator>
    {
        protected List<IDisposable> _disposables = new List<IDisposable>();

        private IContextStateBehaviour<IEnumerator> _state;

        /// <summary>
        /// state local context data
        /// </summary>
        protected IContextProvider<IContext> _context;

        #region public methods

        public void Exit(IContext context)
        {

            var behaviour = GetBehaviour(context);
            behaviour.Exit(context);

        }

        public bool IsActive(IContext context)
        {
            var state = GetBehaviour(context);
            return state.IsActive(context);
        }

        /// <summary>
        /// stop ay execution of state
        /// release all resources
        /// </summary>
        public virtual void Dispose()
        {
            _disposables.Cancel();
            _state?.Dispose();
        }

        public IEnumerator Execute(IContext context)
        {
            StateLogger.LogState(string.Format("STATE EXECUTE {0} TYPE {1}", this.name, GetType().Name), this);
            var state = GetBehaviour(context);
            yield return state.Execute(context);
        }

        #endregion

        #region state behaviour methods

        protected void Initialize(IContextProvider<IContext> stateContext)
        {
            _context = stateContext;
            OnInitialize();
        }

        protected virtual void OnInitialize()
        {

        }

        protected virtual IEnumerator ExecuteState(IContext context)
        {
            yield break;
        }

        protected virtual void OnExit(IContext context) { }


        #endregion

        protected virtual IContextStateBehaviour<IEnumerator> Create()
        {
            var behaviour = new ProxyStateBehaviour();
            behaviour.Initialize(ExecuteState, Initialize, OnExit);
            return behaviour;
        }

        protected virtual IContextStateBehaviour<IEnumerator> GetBehaviour(IContext context)
        {
            if (_state == null)
            {
                _state = Create();
            }

            return _state;
        }
    }
}