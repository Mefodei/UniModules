﻿using System;
using System.Collections;
using Assets.Tools.UnityTools.Interfaces;
using Assets.Tools.UnityTools.StateMachine.ContextStateMachine;
using Assets.Tools.UnityTools.StateMachine.Interfaces;
using Assets.Tools.UnityTools.UniRoutine;
using UnityEngine;

namespace Assets.Tools.UnityTools.StateMachine.UniStateMachine
{
	[Serializable]
	[CreateAssetMenu(menuName = "UniStateMachine/StateMachine", fileName = "StateMachine")]
	public class UniStateMachineObject :
		UniStateBehaviour 
	{
		#region protected methods

		[SerializeField] 
		protected UniStateSelector _stateSelector;

        #endregion

		public IContextSelector<IEnumerator> StateSelector => _stateSelector;

		public void SetSelector(UniStateSelector selector) {
			_stateSelector = selector;
		}
		
		#region private methods

	    protected override IContextStateBehaviour<IEnumerator> Create()
		{
		    var executor = new UniRoutineExecutor();
		    var stateMachine = new ContextStateMachine<IEnumerator>(executor);
            var reactiveState = new ContextReactiveStateMachine();

		    reactiveState.Initialize(_stateSelector, stateMachine);
            return reactiveState;
		}

		#endregion
	}
}