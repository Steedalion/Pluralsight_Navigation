using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coroutines;

public class Character
	: CoroutineBehaviour
{
	[SerializeField]
	Transform _CameraFocus;

	[SerializeField]
	Transform _UIAnchor;
	
	protected Animator _Animator;
	protected int _AttackId;
	protected int _DieId;
	protected int _AttackIndexId;

	public delegate void AnimationCallback();
	public AnimationCallback OnRandomIdleStarted;
	public AnimationCallback OnHit;
	public AnimationCallback OnAttackAnimEnd;
	public AnimationCallback OnAttackEarlyOut;
	public AnimationCallback OnRagdollSwitch;

	public CharacterStats Stats
	{
		get;
		protected set;
	}

	public CharacterStats InitialStats
	{
		get;
		private set;
	}

	public Transform CameraFocus
	{
		get { return _CameraFocus; }
	}

	public Transform UIAnchor
	{
		get { return _UIAnchor; }
	}

	protected override void Awake()
	{
		base.Awake();
		_Animator = GetComponent<Animator>();
		_AttackId = Animator.StringToHash("Attack");
		_DieId = Animator.StringToHash("Die");
		_AttackIndexId = Animator.StringToHash("AttackIndex");
	}

	public void Initialize(ref CharacterStats initialStats)
	{
		Stats = initialStats;
		InitialStats = initialStats;
	}

	public void CancelCurrentCommand()
	{
		CancelCoroutine();
	}

	public virtual void ApplyDamage(float damage)
	{
		// This is temp!
		var newStats = Stats;
		newStats.HP = Mathf.Max(newStats.HP - damage, 0.0f);
		Stats = newStats;
	}

	protected IEnumerable<Instruction> WaitForHitEvent()
	{
		bool hit = false;
		AnimationCallback endEventHandler = () => hit = true;
		OnHit += endEventHandler;
		try
		{
			while (!hit)
				yield return Flow.WaitForAnimatorUpdate;
		}
		finally
		{
			OnHit -= endEventHandler;
		}
	}

	protected IEnumerable<Instruction> WaitForAttackAnimEnd()
	{
		bool hit = false;
		AnimationCallback endEventHandler = () =>
		{
			hit = true;
		};
		OnAttackAnimEnd += endEventHandler;
		try
		{
			while (!hit)
				yield return Flow.WaitForAnimatorUpdate;
		}
		finally
		{
			OnAttackAnimEnd -= endEventHandler;
		}
	}

	protected IEnumerable<Instruction> WaitForRagdollSwitch()
	{
		bool hit = false;
		AnimationCallback endEventHandler = () =>
		{
			hit = true;
		};
		OnRagdollSwitch += endEventHandler;
		try
		{
			while (!hit)
				yield return Flow.WaitForAnimatorUpdate;
		}
		finally
		{
			OnRagdollSwitch -= endEventHandler;
		}
	}

	protected IEnumerable<Instruction> WaitForEarlyOut()
	{
		bool hit = false;
		AnimationCallback endEventHandler = () => hit = true;
		OnAttackEarlyOut += endEventHandler;
		try
		{
			while (!hit)
				yield return Flow.WaitForAnimatorUpdate;
		}
		finally
		{
			OnAttackEarlyOut -= endEventHandler;
		}
	}

	/// <summary>
	/// Animation Event Handlers
	/// </summary>

	void HitEvent()
	{
		if (OnHit != null)
		{
			OnHit.Invoke();
		}
	}

	void EarlyOut()
	{
		if (OnAttackEarlyOut != null)
		{
			OnAttackEarlyOut.Invoke();
		}
	}

	void AttackAnimEnd()
	{
		if (OnAttackAnimEnd != null)
		{
			OnAttackAnimEnd.Invoke();
		}
	}

	void RagdollSwitch()
	{
		if (OnRagdollSwitch != null)
		{
			OnRagdollSwitch.Invoke();
		}
	}
}
