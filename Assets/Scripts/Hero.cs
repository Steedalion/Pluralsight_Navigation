//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Coroutines;
using Messages;
using System.Linq;
using UnityStandardAssets.CrossPlatformInput;

public class Hero
	: Character
{
	[SerializeField]
	float _WalkSpeed = 1.0f;

	[SerializeField]
	float _RunSpeed = 4.0f;

	[SerializeField]
	float _HealSpeed = 1.0f;

	[SerializeField]
	int[] _StandardAttackIndices;

	[SerializeField]
	int[] _FinishingMoveIndices;

	protected override void Awake()
	{
		base.Awake();
		// Temp!
		GetComponent<CharController>().ConstrainAnimationDeltaToNavmesh = true;
	}

	protected override void Update()
	{
		base.Update();

		// Heal!
		if (Stats.HP > 0.0f)
		{
			CharacterStats newStats = Stats;
			newStats.HP = Mathf.Clamp(newStats.HP + _HealSpeed * Time.deltaTime, 0.0f, InitialStats.HP);
			Stats = newStats;
		}
	}

	protected IEnumerable<Instruction> RunTo(RunTo runToCommand)
	{
		return RunTo(runToCommand.Destination, runToCommand.DistanceFromGoal, runToCommand.Callback);
	}

	protected IEnumerable<Instruction> Attack(Attack attackCommand)
	{
		// First run to the target
		return AutoAttack(attackCommand.Target, attackCommand.Callback);
	}

	protected IEnumerable<Instruction> RunToCharacter(Character target, float distanceFromCharacter)
	{
		return GetComponent<CharController>().RunToCharacterCr(target, distanceFromCharacter, _RunSpeed);
	}

	public IEnumerable<Instruction> RunTo(Vector3 destination, float distanceFromGoal, System.Action callback)
	{
		GetComponent<CharacterDirectControl>().ControllingPlayer = false;

		using (var subRoutine = GetComponent<CharController>().RunToCr(destination, distanceFromGoal, _RunSpeed).GetEnumerator())
		{
			while (subRoutine.MoveNext())
				yield return subRoutine.Current;
		}

		if (callback != null)
			callback.Invoke();
	}

	public IEnumerable<Instruction> AutoAttack(Character target, System.Action callback)
	{
		// First run to the target

		// Figure out how far exactly
		float distance = 1.0f;
		float ignore = 0.0f;
		CombatManager.Instance.ComputeDistances(this, target, ref distance, ref ignore);

		// Run there!
		using (var runToChar = RunToCharacter(target, distance).GetEnumerator())
		{
			while (runToChar.MoveNext())
				yield return runToChar.Current;
		}

		Character currentTarget = target;
		while (currentTarget != null)
		{
			// Snap to face the target if necessary!
			Vector3 deltaToTarget = target.transform.position - transform.position;
			deltaToTarget.y = 0.0f;
			transform.rotation = Quaternion.LookRotation(deltaToTarget);

			// Wait a frame for the rotation to stick before driving it with anim deltas!
			yield return null;

			// Kill the curent target
			using (var attackTarget = AttackTargetInPlace(currentTarget, () => IsTargetAttackable(currentTarget), () => true, () => { }).GetEnumerator())
			{
				while (attackTarget.MoveNext())
					yield return attackTarget.Current;
			}

			// If somebody else is attacking us, switch to them!
			currentTarget = CombatManager.Instance.AggroedMonsters.FirstOrDefault(monster => monster != currentTarget);

			yield return null;
		}

		if (callback != null)
			callback.Invoke();
	}

	IEnumerable<Instruction> AttackTargetInPlace(Character target, System.Func<bool> targetValid, System.Func<bool> attackQueued, System.Action attackTriggered)
	{
		// Then play the attack animation, and apply damage until the other character is dead
		int index = 0;
		while (target.Stats.HP > 0.0f && targetValid() && attackQueued())
		{
			// Play one attack!
			attackTriggered();

			// Determine if we want to chain attacks or not
			bool attackWillNotKill = false;
			int attackAnimIndex = _StandardAttackIndices[index];

			// Will the attack kill?
			float expectedDamage = CombatFormulas.ComputeDamage(Stats, target.Stats);
			if (expectedDamage < target.Stats.HP)
			{
				// No, so early out in order to play next anim!
				// We also don't play a special attack if the target will die in one hit!
				attackWillNotKill = true;
			}
			else
			{
				// Yes, pick a random finishing attack
				// Special case for one-hit kills!
				if (index > 0)
				{
					attackAnimIndex = _FinishingMoveIndices[Random.Range(0, _FinishingMoveIndices.Length)];
				}
			}

			// This will be called on the hit frame
			System.Action onHitCallback = () =>
			{
				// Once the hitFrame is triggered, compute damage and apply it!
				float damage = CombatFormulas.ComputeDamage(Stats, target.Stats);
				Game.Instance.PushMessage(Messages.ApplyDamage.Create(target, this, damage));
			};

			System.Func<bool> earlyOutCheck = () =>
			{
				return attackWillNotKill && attackQueued();
			};

			// Play a single attack
			using (var attack = AttackOnce(attackAnimIndex, onHitCallback, earlyOutCheck).GetEnumerator())
			{
				while (attack.MoveNext())
					yield return attack.Current;
			}

			// If we earlied out of the attack, then pick a new attack index
			if (attackWillNotKill)
			{
				index++;
				if (index == _StandardAttackIndices.Length)
					index = 0;
			}
		}
	}

	public IEnumerable<Instruction> MonitorForAttacksOnTargets()
	{
		Monster currentBestTarget = null;
		var attackOnPress = AttackBestTargetOnButtonPress(() => currentBestTarget);
		var pickBestTarget = FocusOnBestTarget(m => currentBestTarget = m);
		using (var concurrent = Flow.MasterSlave(attackOnPress, pickBestTarget).GetEnumerator())
		{
			while (concurrent.MoveNext())
				yield return concurrent.Current;
		}
	}

	public IEnumerable<Instruction> AttackBestTargetOnButtonPress(System.Func<Monster> getCurrentBestTarget)
	{
		while (true)
		{
			Monster currentTarget = null;
			if (CrossPlatformInputManager.GetButtonDown("Fire1"))
			{
				// Do we have a potential target to attack?
				currentTarget = getCurrentBestTarget();
				if (currentTarget != null)
				{
					// Yes, go ahead and attack it!
					try
					{
						//currentTarget.Targetted = true;
						// We have a target, attack it!
						using (var attackManual = ManualAttackTargetInPlace(currentTarget).GetEnumerator())
						{
							while (attackManual.MoveNext())
								yield return attackManual.Current;
						}
					}
					finally
					{
						//currentTarget.Targetted = false;
					}
				}
				else
				{
					// No actual target, just swing!
					using (var attackEmpty = AttackOnce(0, null, () => false).GetEnumerator())
					{
						while (attackEmpty.MoveNext())
							yield return attackEmpty.Current;

					}
				}
			}
			yield return null;
		}
	}

	IEnumerable<Instruction> FocusOnBestTarget(System.Action<Monster> setNewTarget)
	{
		Monster currentBest = null;
		List<Monster> potentialTargets = new List<Monster>();
		while (true)
		{
			potentialTargets.Clear();
			// Do we have a target?
			foreach (var monster in ObjectManager.Instance.Monsters)
			{
				if (IsTargetAttackable(monster) && monster.Stats.HP > 0.0f)
				{
					potentialTargets.Add(monster);
				}
			}

			Monster newTarget = null;
			if (potentialTargets.Count > 0)
			{
				// Sort according to the angle to the target!
				potentialTargets.OrderBy(m =>
				{
					Vector3 delta = m.transform.position - transform.position; delta.y = 0.0f;
					return Mathf.Abs(Vector3.Angle(delta, transform.forward));
				});
				newTarget = potentialTargets.First();
			}

			if (newTarget != currentBest)
			{
				if (currentBest != null)
				{
					currentBest.Highlighted = false;
				}
				currentBest = newTarget;

				if (currentBest != null)
				{
					currentBest.Highlighted = true;
				}
				setNewTarget(currentBest);
			}

			// Wait until next frame!
			yield return null;
		}
	}

	bool IsTargetInRange(Character target)
	{
		// Fetch values
		float bestDist = 0.0f;
		float distVar = 0.0f;
		CombatManager.Instance.ComputeDistances(this, target, ref bestDist, ref distVar);

		// Compute target distance and angle
		Vector3 delta = target.transform.position - transform.position;
		delta.y = 0.0f;
		float distance = delta.magnitude;

		return distance < (bestDist * 2.0f);
	}

	bool IsTargetAttackable(Character target)
	{
		// Fetch values
		float bestDist = 0.0f;
		float distVar = 0.0f;
		CombatManager.Instance.ComputeDistances(this, target, ref bestDist, ref distVar);
		float maxAngle = CombatManager.Instance.ComputeMaxAngle(this, target);

		// Compute target distance and angle
		Vector3 delta = target.transform.position - transform.position;
		delta.y = 0.0f;
		float distance = delta.magnitude;
		float angle = Mathf.Abs(Vector3.Angle(delta, transform.forward));

		return distance < (bestDist * 2.0f) && angle < (maxAngle * 2.0f);
	}

	IEnumerable<Instruction> ManualAttackTargetInPlace(Character target)
	{
		// Snap to face the target if necessary!
		Vector3 deltaToTarget = target.transform.position - transform.position;
		deltaToTarget.y = 0.0f;
		transform.rotation = Quaternion.LookRotation(deltaToTarget);

		// This will store whether the player hit a button to
		// The value starts true because we hit a button to get to this state!
		bool attackQueued = true;

		System.Action setAttackQueued = () =>
			{
				attackQueued = true;
			};

		System.Func<bool> queryAttackQueued = () =>
			{
				return attackQueued;
			};

		System.Action resetAttackQueued = () =>
			{
				attackQueued = false;
			};

		// Execute the attack while we check the input for another queued attack
		using (var attackWhile = Flow.MasterSlave(
			AttackTargetInPlace(target, () => IsTargetAttackable(target), queryAttackQueued, resetAttackQueued),
			MonitorAttackButton(setAttackQueued)).GetEnumerator())
		{
			while (attackWhile.MoveNext())
				yield return attackWhile.Current;
		}
	}

	public IEnumerable<Instruction> MonitorAttackButton(System.Action attackQueued)
	{
		yield return null;
		yield return null;
		// Constantly poll the input!
		while (true)
		{
			if (CrossPlatformInputManager.GetButtonDown("Fire1"))
			{
				attackQueued();
			}
			yield return null;
		}
	}

	public IEnumerable<Instruction> AttackOnce(int attackIndex, System.Action onHit, System.Func<bool> earlyOut)
	{
		_Animator.SetInteger(_AttackIndexId, attackIndex);
		_Animator.SetBool(_AttackId, true);
		_Animator.SetLayerWeight(2, 1.0f);

		try
		{
			using (var hitEvent = WaitForHitEvent().GetEnumerator())
			{
				while (hitEvent.MoveNext())
					yield return hitEvent.Current;
			}

			// Apply the damage
			if (onHit != null)
			{
				onHit.Invoke();
			}

			_Animator.SetBool(_AttackId, false);

			if (earlyOut())
			{
				// Wait until early out event
				using (var earlyOutEvent = WaitForEarlyOut().GetEnumerator())
				{
					while (earlyOutEvent.MoveNext())
						yield return earlyOutEvent.Current;
				}
			}
			else
			{
				// Wait until the end of the anim!
				using (var endEvent = WaitForAttackAnimEnd().GetEnumerator())
				{
					while (endEvent.MoveNext())
						yield return endEvent.Current;
				}
			}
		}
		finally
		{
			_Animator.SetBool(_AttackId, false);
			_Animator.SetLayerWeight(2, 0.0f);
		}
	}

	public IEnumerable<Instruction> Die(System.Action callback)
	{
		// Play death anim!
		_Animator.SetLayerWeight(1, 1.0f);
		_Animator.SetLayerWeight(2, 0.0f);

		// Kick off the animation
		_Animator.SetTrigger(_DieId);

		// Wait until the end of the anim!
		using (var endEvent = WaitForRagdollSwitch().GetEnumerator())
		{
			while (endEvent.MoveNext())
				yield return endEvent.Current;
		}

		if (callback != null)
			callback.Invoke();
	}

	public IEnumerable<Instruction> WalkThroughDoorway(Doorway doorway)
	{
		using (var runToEntry = GetComponent<CharController>().RunToCr(doorway.StartMarker.transform.position, 0.0f, _RunSpeed).GetEnumerator())
		{
			while (runToEntry.MoveNext())
				yield return runToEntry.Current;
		}

		using (var runToExit = GetComponent<CharController>().RunToCr(doorway.EndMarker.transform.position, 0.0f, _RunSpeed).GetEnumerator())
		{
			while (runToExit.MoveNext())
				yield return runToExit.Current;
		}
	}
}
