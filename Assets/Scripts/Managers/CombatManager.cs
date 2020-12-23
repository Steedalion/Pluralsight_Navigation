using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum CombatPositionType
{
	Melee,
	MeleeWait,
}

public class CombatPosition
{
	public int Index
	{
		get;
		private set;
	}

	public CombatPositionType PositionType
	{
		get;
		private set;
	}


	public CombatPosition(int index, CombatPositionType type)
	{
		Index = index;
		PositionType = type;
	}

	public Vector3 ComputePosition(Vector3 center)
	{
		return CombatManager.Instance.ComputePosition(center, Index);
	}

	public void Dispose()
	{
		if (Index != -1)
		{
			CombatManager.Instance.ReturnPosition(this);
			Index = -1;
		}
	}
}

public class CombatManager
	: SingletonBehaviour<CombatManager>
	, IManager
{
	HashSet<Monster> _AggroedMonsters;
	int _PositionMask;

	public int KillCount
	{
		get;
		private set;
	}

	Queue<MonsterSpawn> _AvailableSpawns;

	public bool IsInCombat
	{
		get
		{
			return _AggroedMonsters.Count > 0;
		}
	}

	public IEnumerable<Monster> AggroedMonsters
	{
		get { return _AggroedMonsters; }
	}

	public int IndexCount
	{
		get
		{
			return Mathf.FloorToInt(360.0f / Globals.Instance.Settings.AngleBetweenPositions);
		}
	}

	public int HoldingIndexCount
	{
		get
		{
			return Mathf.FloorToInt(360.0f / Globals.Instance.Settings.AngleBetweenHoldingPositions);
		}
	}

	public void Initialize()
	{
		// Nothing for now!
		_AggroedMonsters = new HashSet<Monster>();
		_AvailableSpawns = new Queue<MonsterSpawn>();
	}

	public void Reset()
	{
		_AggroedMonsters.Clear();
		_AvailableSpawns.Clear();
		KillCount = 0;
	}

	public void Process()
	{
	}

	public void MonsterAggro(Monster monster)
	{
		// If this was the first monster, notify the game that "combat" started!
		if (_AggroedMonsters.Count == 0)
		{
			Game.Instance.PushMessage(Messages.CombatStarted.Create());
		}

		_AggroedMonsters.Add(monster);
	}

	public void MonsterDead(Monster monster)
	{
		if (_AggroedMonsters.Remove(monster))
		{
			KillCount++;
			// If we removed the last monster, notify the game that "combat" ended
			if (_AggroedMonsters.Count == 0)
			{
				Game.Instance.PushMessage(Messages.CombatEnded.Create());
			}
		}
	}

	public Vector3 ComputePosition(Vector3 center, int index)
	{
		int indexCount = IndexCount;
		if (index >= indexCount)
		{
			float angle = Globals.Instance.Settings.AngleBetweenHoldingPositions * (index - indexCount) * Mathf.Deg2Rad;
			Vector3 unitDelta = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
			return center + unitDelta * Globals.Instance.Settings.MonsterHoldingDistance;
		}
		else
		{
			float angle = Globals.Instance.Settings.AngleBetweenPositions * index * Mathf.Deg2Rad;
			Vector3 unitDelta = new Vector3(Mathf.Cos(angle), 0.0f, Mathf.Sin(angle));
			return center + unitDelta * Globals.Instance.Settings.MonsterDistance;
		}
	}


	IEnumerable<CombatPosition> EnumerateSortedPositionIndices(Vector3 direction)
	{
		int indexCount = IndexCount;
		int holdingIndexCount = HoldingIndexCount;

		float angle = Mathf.Repeat(Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg, 360.0f);

		// Find the closest index
		int refIndex = Mathf.RoundToInt(angle / Globals.Instance.Settings.AngleBetweenPositions);
		yield return new CombatPosition(refIndex, CombatPositionType.Melee);

		for (int i = 0; i < (indexCount / 2) - 1; ++i)
		{
			// Right side
			int potentialIndex = (refIndex + i + 1) % indexCount;
			yield return new CombatPosition(potentialIndex, CombatPositionType.Melee);

			// Left side
			potentialIndex = (refIndex + indexCount - i - 1) % indexCount;
			yield return new CombatPosition(potentialIndex, CombatPositionType.Melee);
		}

		// No melee position, repeat the process for holding positions
		refIndex = Mathf.RoundToInt(angle / Globals.Instance.Settings.AngleBetweenHoldingPositions);
		yield return new CombatPosition((refIndex + indexCount), CombatPositionType.MeleeWait);

		for (int i = 0; i < (holdingIndexCount / 2) - 1; ++i)
		{
			// Right side
			int potentialIndex = indexCount + (refIndex + i + 1) % holdingIndexCount;
			yield return new CombatPosition(potentialIndex, CombatPositionType.MeleeWait);

			// Left side
			potentialIndex = indexCount + (refIndex + holdingIndexCount - i - 1) % holdingIndexCount;
			yield return new CombatPosition(potentialIndex, CombatPositionType.MeleeWait);
		}
	}

	public CombatPosition ReserveClosestPosition(Vector3 center, Vector3 refPos)
	{
		Vector3 delta = refPos - center;
		foreach (var pos in EnumerateSortedPositionIndices(delta))
		{
			// Check if the position isn't already reserved
			if ((_PositionMask & (1 << pos.Index)) == 0)
			{
				// Check if the position is valid
				NavMeshHit meshHit;
				if (NavMesh.SamplePosition(pos.ComputePosition(center), out meshHit, Globals.Instance.Settings.NavMeshDistance, NavMesh.AllAreas))
				{
					// Reserve the position and return it
					_PositionMask |= (1 << pos.Index);
					return pos;
				}
			}
		}

		return null;
	}

	public void ReturnPosition(CombatPosition position)
	{
		if (position is CombatPosition)
		{
			_PositionMask &= ~(1 << (position as CombatPosition).Index);
		}
	}

	public void ComputeDistances(Character subject, Character target, ref float outDistance, ref float outVar)
	{
		outDistance = Globals.Instance.Settings.MonsterDistance;
		outVar = 0.5f;
	}

	public float ComputeMaxAngle(Character subject, Character target)
	{
		return Globals.Instance.Settings.AngleBetweenPositions * 0.5f;
	}
}
