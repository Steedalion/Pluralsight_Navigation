using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coroutines;

public class MonsterSpawnCoordinator
	: CoroutineBehaviour
	, IResetable
{
	[SerializeField]
	int[] _MonstersPerWave;

	[SerializeField]
	float _TimeBetweenSpawns = 0.5f;

	[SerializeField]
	float _DetectionDistance = 15.0f;

	List<MonsterSpawn> _Spawners;

	// Use this for initialization
	// Use this for initialization
	public void Init()
	{
		_Spawners = new List<MonsterSpawn>(GetComponentsInChildren<MonsterSpawn>());
		SetCoroutine(Main());
	}

	public void Dispose()
	{
		CancelCoroutine();
	}

	// Update is called once per frame
	IEnumerable<Instruction> Main()
	{
		int waveIndex = 0;
		while (true)
		{
			// Wait for hero to be close enough
			float distance = float.MaxValue;
			while (distance > _DetectionDistance)
			{
				yield return null;
				distance = Vector3.Distance(transform.position, Game.Instance.Hero.transform.position);
			}

			// Create a randomized list of spawns, so the monsters don't always spawn in the same order
			List<MonsterSpawn> randomizedSpawns = new List<MonsterSpawn>();
			randomizedSpawns.Add(_Spawners[0]);
			for (int i = 1; i < _MonstersPerWave[waveIndex]; ++i)
			{
				int spawnerIndex = i % _Spawners.Count;
				randomizedSpawns.Insert(Random.Range(0, randomizedSpawns.Count), _Spawners[spawnerIndex]);
			}

			// Spawn the monsters, and keep track of them!
			List<Monster> monsters = new List<Monster>();
			for (int i = 0; i < randomizedSpawns.Count; ++i)
			{
				// Spawn the monster and remember it!
				using (var spawnOnce = randomizedSpawns[i].SpawnOnce(m => monsters.Add(m)).GetEnumerator())
				{
					while (spawnOnce.MoveNext())
						yield return spawnOnce.Current;
				}

				// Wait a bit!
				using (var wait = Flow.WaitForSeconds(_TimeBetweenSpawns).GetEnumerator())
				{
					while (wait.MoveNext())
						yield return wait.Current;
				}
			}

			// Wait for all the monsters to die!
			while (monsters.Count > 0)
			{
				// This should hook into the event system instead!
				for (int i = 0; i < monsters.Count; ++i)
				{
					var monster = monsters[i];
					if (monster.Stats.HP <= 0.0f)
					{
						monsters.RemoveAt(i);
						i--;
					}
				}

				// Wait until next frame
				yield return null;
			}

			if (waveIndex < _MonstersPerWave.Length - 1)
			{
				waveIndex++;
			}
		}
	}
}
