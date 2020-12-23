using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Base behaviour class that knows how to execute coroutines and handle the different yield instructions
/// </summary>
public class CoroutineBehaviour
	: MonoBehaviour
{
	Coroutines.CoroutineSite _MainCoroutine;

	protected virtual void Awake()
	{
		_MainCoroutine = new Coroutines.CoroutineSite();
	}

	protected virtual void Update()
	{
		_MainCoroutine.Update();
	}

	protected virtual void LateUpdate()
	{
		_MainCoroutine.LateUpdate();
	}

	protected virtual void OnAnimatorMove()
	{
		_MainCoroutine.OnAnimatorMove();
	}

	public void SetCoroutine(IEnumerable<Coroutines.Instruction> coroutine)
	{
		_MainCoroutine.SetCoroutine(coroutine);
	}

	public void CancelCoroutine()
	{
		_MainCoroutine.CancelCoroutine();
	}
}
