using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.AI;
using Coroutines;
using Messages;

public class Monster
	: Character
	, WorldMouseManager.ITarget
{
	[SerializeField]
	Rigidbody _RootRigidBody;

	[SerializeField]
	float _WalkSpeed = 1.0f;

	[SerializeField]
	float _RunSpeed = 2.0f;

	[SerializeField]
	float _OnDeathUpMagnitude = 0.5f;

	[SerializeField]
	float _DeathForceMagnitude = 5000.0f;

	[SerializeField]
	float _DeathForceTorque = -50.0f;

	[SerializeField]
	float _AggroDistance = 5.0f;

	CharController _CharController;

	struct ReplacementMaterials
	{
		public Material[] StandardMaterials;
		public Material[] HighlightedMaterials;
		public Material[] TargettedMaterials;
	}

	Dictionary<Renderer, ReplacementMaterials> _MeshesAndMaterials;

	// Temp
	bool _ForceAggro;
	CombatPosition _CurrentPosition;
	bool _Targetted;
	bool _Highlighted;

	public bool Highlighted
	{
		get { return _Highlighted; }
		set
		{
			if (_Highlighted != value)
			{
				_Highlighted = value;
				UpdateMaterials();
			}
		}
	}

	public bool Targetted
	{
		get { return _Targetted; }
		set
		{
			if (_Targetted != value)
			{
				_Targetted = value;
				UpdateMaterials();
			}
		}
	}

	void UpdateMaterials()
	{
		foreach (var rendererAndMats in _MeshesAndMaterials)
		{
			if (Targetted)
			{
				rendererAndMats.Key.sharedMaterials = rendererAndMats.Value.TargettedMaterials;
			}
			else if (Highlighted)
			{
				rendererAndMats.Key.sharedMaterials = rendererAndMats.Value.HighlightedMaterials;
			}
			else
			{
				rendererAndMats.Key.sharedMaterials = rendererAndMats.Value.StandardMaterials;
			}
		}
	}


	protected override void Awake()
	{
		base.Awake();
		_CharController = GetComponent<CharController>();
	}

	void Start()
	{
		var allrbs = GetComponentsInChildren<Rigidbody>();
		foreach (var rb in allrbs)
		{
			rb.isKinematic = true;
		}

		_MeshesAndMaterials = new Dictionary<Renderer, ReplacementMaterials>();
		foreach (var renderer in GetComponentsInChildren<Renderer>())
		{
			ReplacementMaterials mats = new ReplacementMaterials();

			// Setup materials for highlight and normal
			mats.StandardMaterials = renderer.sharedMaterials;
			mats.HighlightedMaterials = new Material[mats.StandardMaterials.Length + Globals.Instance.Settings.HighlightMaterials.Length];
			mats.TargettedMaterials = new Material[mats.StandardMaterials.Length + Globals.Instance.Settings.HighlightMaterials.Length];
			for (int i = 0; i < mats.StandardMaterials.Length; ++i)
			{
				mats.HighlightedMaterials[i] = mats.StandardMaterials[i];
				mats.TargettedMaterials[i] = mats.StandardMaterials[i];
			}
			for (int i = 0; i < Globals.Instance.Settings.HighlightMaterials.Length; ++i)
			{
				mats.HighlightedMaterials[mats.StandardMaterials.Length + i] = Globals.Instance.Settings.HighlightMaterials[i];
			}
			for (int i = 0; i < Globals.Instance.Settings.TargettedMaterials.Length; ++i)
			{
				mats.TargettedMaterials[mats.StandardMaterials.Length + i] = Globals.Instance.Settings.TargettedMaterials[i];
			}

			_MeshesAndMaterials.Add(renderer, mats);
		}
	}

	public override void ApplyDamage(float damage)
	{
		base.ApplyDamage(damage);

		// This should trigger the monster to aggro!
		_ForceAggro = true;
	}

	public IEnumerable<Instruction> CreepToHeroThenAttack()
	{
		// Run to the marker until we detect the hero!
		using (var runWhile = Flow.ExecuteWhile(CreepToHero(), () => !DetectHero()).GetEnumerator())
		{
			while (runWhile.MoveNext())
				yield return runWhile.Current;
		}

		// Then switch to standard AI, which will re-detect the hero and attack him!
		using (var aimain = AIMain().GetEnumerator())
		{
			while (aimain.MoveNext())
				yield return aimain.Current;
		}
	}

	public IEnumerable<Instruction> CreepTo(Transform marker)
	{
		// Run to the marker until we detect the hero!
		var runTo = _CharController.RunToCr(marker.transform.position, 0.0f, _WalkSpeed);
		using (var runWhile = Flow.ExecuteWhile(runTo, () => !DetectHero()).GetEnumerator())
		{
			while (runWhile.MoveNext())
				yield return runWhile.Current;
		}

		// Then switch to standard AI, which will re-detect the hero and attack him!
		using (var aimain = AIMain().GetEnumerator())
		{
			while (aimain.MoveNext())
				yield return aimain.Current;
		}
	}

	bool DetectHero()
	{
		float distToHero = Vector3.Distance(transform.position, Game.Instance.Hero.transform.position);
		return distToHero < _AggroDistance;
	}

	public IEnumerable<Instruction> AIMain()
	{
		var hero = Game.Instance.Hero;

		// Figure out the distances
		float dist = 1.0f;
		float distVar = 0.0f;
		CombatManager.Instance.ComputeDistances(this, hero, ref dist, ref distVar);

		// Wait until the hero is close enough
		float distToHero = Vector3.Distance(transform.position, hero.transform.position);
		while (distToHero > _AggroDistance && !_ForceAggro)
		{
			yield return null;
			distToHero = Vector3.Distance(transform.position, hero.transform.position);
		}

		// Notify that we aggro the player
		Game.Instance.PushMessage(MonsterAggro.Create(this, hero));

		// Follow the player and attack him forever!
		try
		{
			while (hero.Stats.HP > 0.0f)
			{
				// Run to the hero
				using (var runToHero = Flow.ExecuteOrWaitUntil(RunToHero()).GetEnumerator())
				{
					while (runToHero.MoveNext())
						yield return runToHero.Current;
				}

				if (_CurrentPosition.PositionType == CombatPositionType.Melee)
				{
					// Attack the hero, so long as he is close enough!
					using (var attackWhile = Flow.ExecuteWhile(AttackHero(),
						() => Vector3.Distance(transform.position, hero.transform.position) < (dist + distVar)).GetEnumerator())
					{
						while (attackWhile.MoveNext())
							yield return attackWhile.Current;
					}
				}
				// Else keep trying to find a better position
			}
		}
		finally
		{
			if (_CurrentPosition != null)
			{
				_CurrentPosition.Dispose();
			}
		}
	}

	IEnumerable<Instruction> RunToHero()
	{
		var hero = Game.Instance.Hero;
		float distDelta = 0.25f;

		// Path if we're not close enough to the target
		float distanceFromTarget = float.MaxValue;
		do
		{
			// Path to the target, and repath if the target moves too far!
			Vector3 heroStartPos = hero.transform.position;

			// Return current position if any
			if (_CurrentPosition != null)
			{
				CombatManager.Instance.ReturnPosition(_CurrentPosition);
			}

			// Compute how far we want to be
			float dist = 1.0f;
			float distVar = 0.0f;
			CombatManager.Instance.ComputeDistances(this, hero, ref dist, ref distVar);

			// Build a first path to the target, so we know the direction we're coming from
			Vector3 referencePosition = transform.position;
			NavMeshPath referencePath = new NavMeshPath();
			if (NavMesh.CalculatePath(transform.position, hero.transform.position, NavMesh.AllAreas, referencePath))
			{
				float ignore = 0.0f;
				NavMeshUtils.ComputeLocationForDistanceFromGoal(referencePath.corners, dist, out referencePosition, out ignore);
			}

			// Grab new position from the reference point, and path there!
			_CurrentPosition = CombatManager.Instance.ReserveClosestPosition(heroStartPos, referencePosition);
			if (_CurrentPosition != null)
			{
				// Compute the point to path to!
				Vector3 targetPos = _CurrentPosition.ComputePosition(heroStartPos);

				// Run there, but break out (so we can repath) if the player moves too far from their position
				System.Func<bool> heroMovedPredicate = () =>
				{
					Vector3 heroPos = hero.transform.position;
					float heroMoveDist = Vector3.Distance(heroPos, heroStartPos);
					return heroMoveDist < distDelta;
				};

				using (var run = Flow.ExecuteOrWaitUntil(Flow.ExecuteWhile(_CharController.RunToCr(targetPos, 0.0f, _RunSpeed), heroMovedPredicate)).GetEnumerator())
				{
					while (run.MoveNext())
						yield return run.Current;
				}

				// Update target pos and distance
				targetPos = _CurrentPosition.ComputePosition(heroStartPos);
				distanceFromTarget = Vector3.Distance(targetPos, transform.position);
			}
			else
			{
				yield return null;
			}
		}
		while (distanceFromTarget > distDelta);
	}

	IEnumerable<Instruction> AttackHero()
	{
		// Compute the gamage we'll apply
		var hero = Game.Instance.Hero;

		// Perform the attack!
		System.Action damageCallback = () =>
		{
			float damage = CombatFormulas.ComputeDamage(Stats, hero.Stats);
			Game.Instance.PushMessage(Messages.ApplyDamage.Create(hero, this, damage));
		};

		while (hero.Stats.HP > 0.0f)
		{
			// Snap to face the target if necessary!
			Vector3 deltaToTarget = hero.transform.position - transform.position;
			deltaToTarget.y = 0.0f;
			transform.rotation = Quaternion.LookRotation(deltaToTarget);

			using (var attack = AttackOnce(0, damageCallback).GetEnumerator())
			{
				while (attack.MoveNext())
					yield return attack.Current;
			}
		}
	}

	public IEnumerable<Instruction> AttackOnce(int attackIndex, System.Action onHit)
	{
		_Animator.SetBool(_AttackId, true);

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

			// Wait until the end of the anim!
			using (var endEvent = WaitForAttackAnimEnd().GetEnumerator())
			{
				while (endEvent.MoveNext())
					yield return endEvent.Current;
			}
		}
		finally
		{
			_Animator.SetBool(_AttackId, false);
		}
	}

	public IEnumerable<Instruction> Die(System.Action callback)
	{
		// Turn off the animator so rbs can simulate!
		_Animator.enabled = false;

		var allrbs = GetComponentsInChildren<Rigidbody>();
		foreach (var rb in allrbs)
		{
			rb.isKinematic = false;
			//rb.ResetInertiaTensor();
		}

		// Disable Mouse Collider
		GetComponent<Collider>().enabled = false;

		// Compute direction from hero
		Vector3 delta = transform.position - Game.Instance.Hero.transform.position;
		delta.y += _OnDeathUpMagnitude;
		delta.Normalize();

		float magnitude = _DeathForceMagnitude;
		float torqueMag = _DeathForceTorque;

		_RootRigidBody.AddForce(delta * magnitude, ForceMode.Force);
		_RootRigidBody.AddRelativeTorque(torqueMag, 0.0f, 0.0f, ForceMode.Force);

		//Game.Instance.PushMessage(RecycleMonster.Create(this));

		// Wait 5 seconds and freeze physics
		using (var wait = Flow.WaitForSeconds(5.0f).GetEnumerator())
		{
			while (wait.MoveNext())
				yield return wait.Current;
		}

		foreach (var rb in allrbs)
		{
			rb.isKinematic = true;
			//rb.ResetInertiaTensor();
		}

		if (callback != null)
			callback.Invoke();
	}

	IEnumerable<Instruction> CreepToHero()
	{
		var hero = Game.Instance.Hero;
		float distDelta = 0.25f;

		// Path if we're not close enough to the target
		float distanceFromTarget = float.MaxValue;
		do
		{
			// Path to the target, and repath if the target moves too far!
			Vector3 heroStartPos = hero.transform.position;

			// Compute the point to path to!
			Vector3 targetPos = heroStartPos;

			// Run there, but break out (so we can repath) if the player moves too far from their position
			System.Func<bool> heroMovedPredicate = () =>
			{
				if (Game.Instance.Hero != null)
				{
					Vector3 heroPos = Game.Instance.Hero.transform.position;
					float heroMoveDist = Vector3.Distance(heroPos, heroStartPos);
					return heroMoveDist < distDelta;
				}
				else
				{
					return false;
				}
			};

			using (var run = Flow.ExecuteOrWaitUntil(Flow.ExecuteWhile(_CharController.RunToCr(targetPos, 0.0f, _WalkSpeed), heroMovedPredicate)).GetEnumerator())
			{
				while (run.MoveNext())
					yield return run.Current;
			}

			// Update target pos and distance
			if (Game.Instance.Hero != null)
			{
				targetPos = hero.transform.position;
				distanceFromTarget = Vector3.Distance(targetPos, transform.position);
			}
			else
			{
				break;
			}
		}
		while (distanceFromTarget > distDelta);
	}


}
