using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager
	: SingletonBehaviour<ObjectManager>
	, IManager
{
	// Instantiated Objects
	List<Monster> _Monsters;

	Queue<Monster> _DeadMonsters;

	public IEnumerable<Monster> Monsters
	{
		get { return _Monsters; }
	}

	public Hero Hero
	{
		get;
		private set;
	}

	public Prop HeroSword
	{
		get;
		private set;
	}

	public void Initialize()
	{
		_Monsters = new List<Monster>();
		_DeadMonsters = new Queue<Monster>();
	}

	public void Process()
	{
		// Nothing to do for now!
	}

	public Hero InstantiateHero(Vector3 pos, Quaternion rot, Hero prefab)
	{
		// Instantiate a hero!
		var ret = GameObject.Instantiate<Hero>(prefab, pos, rot);
		ret.transform.localScale = Vector3.one;
		Hero = ret;
		ret.name = "The Hero";
		return ret;
	}

	public Prop InstantiateProp(Vector3 pos, Quaternion rot, Prop prefab)
	{
		// Instantiate a sword!
		var ret = GameObject.Instantiate<Prop>(prefab, pos, rot);
		ret.transform.localScale = Vector3.one;
		ret.name = "Great Sword";
		return ret;
	}

	public Monster InstantiateMonster(Vector3 pos, Quaternion rot, MonsterTemplate template)
	{
		var ret = GameObject.Instantiate<Monster>(template.Prefab, pos, rot);
		_Monsters.Add(ret);
		ret.transform.localScale = Vector3.one;
		ret.name = "A Monster";
		ret.Initialize(ref template.Stats);
		return ret;
	}

	public void RecycleMonster(Monster monster)
	{
		_Monsters.Remove(monster);
		_DeadMonsters.Enqueue(monster);

		// Destroy Monsters
		if (_DeadMonsters.Count >= Globals.Instance.Settings.MaxDeadMonsterCount)
		{
			var destroyMonster = _DeadMonsters.Dequeue();
			GameObject.Destroy(destroyMonster.gameObject);
		}
	}

	public void RecycleHero()
	{
		GameObject.Destroy(Hero.gameObject);
		Hero = null;
	}

	public void Reset()
	{
		foreach (var monster in _Monsters)
		{
			GameObject.Destroy(monster.gameObject);
		}

		foreach (var monster in _DeadMonsters)
		{
			GameObject.Destroy(monster.gameObject);
		}
		_Monsters.Clear();
		_DeadMonsters.Clear();
	}
}
