﻿using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Tools.UnityTools.Interfaces;
using Assets.Tools.UnityTools.StateMachine.UniStateMachine;
using UnityEngine;

[CreateAssetMenu(menuName = "States/Validators/TimeDelayValidator", fileName = "TimeDelayValidator")]
public class TimeDelayValidator : UniTransitionValidator 
{
	[NonSerialized]
	private float _lastValidationTime;
	
	[SerializeField]
	private float DelayBeforeTransition = 0.2f;

	protected override bool ValidateNode(IContext context) 
	{
		var timePassed =  Time.realtimeSinceStartup - _lastValidationTime;
		_lastValidationTime = Time.realtimeSinceStartup;
		
		if (timePassed < DelayBeforeTransition || _lastValidationTime<=0) {
			return false;
		}

		//reset time
		_lastValidationTime = 0f;
		
		return true;
		
	}
}
