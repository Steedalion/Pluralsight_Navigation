using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
	/// <summary>
	/// Base interface for all lifecycle messages so they are easy to filter out
	/// </summary>
	public interface ISpawnMessage<ObjectType>
		: IMessage
	{
		System.Action<ObjectType> Callback { get; set; }
	}

	/// <summary>
	/// Helper class to create pooled character messages without having to reimplement the interface every time
	/// </summary>
	public class PooledSpawnMessage<T, ObjectType>
		: PooledMessage<T>
		, ISpawnMessage<ObjectType>
		where T : class, ISpawnMessage<ObjectType>, new()
	{
		public System.Action<ObjectType> Callback { get; set; }

		public static T Create(System.Action<ObjectType> callback)
		{
			var ret = Create();
			ret.Callback = callback;
			return ret;
		}
	}

	public class SpawnHero
		: PooledSpawnMessage<SpawnHero, Hero>
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public HeroTemplate Template;

		public static SpawnHero Create(Vector3 pos, Quaternion rot, HeroTemplate template, System.Action<Hero> callback = null)
		{
			var ret = Create(callback);
			ret.Position = pos;
			ret.Rotation = rot;
			ret.Template = template;
			return ret;
		}
	}

	public class SpawnMonster
		: PooledSpawnMessage<SpawnMonster, Monster>
	{
		public Vector3 Position;
		public Quaternion Rotation;
		public MonsterTemplate Template;

		public static SpawnMonster Create(Vector3 pos, Quaternion rot, MonsterTemplate template, System.Action<Monster> callback = null)
		{
			var ret = Create(callback);
			ret.Position = pos;
			ret.Rotation = rot;
			ret.Template = template;
			return ret;
		}
	}

	public class SpawnProp
		: PooledSpawnMessage<SpawnProp, Prop>
	{
		public Prop PropPrefab;
		public Vector3 Position;
		public Quaternion Rotation;

		public static SpawnProp Create(Vector3 pos, Quaternion rot, Prop prefab, System.Action<Prop> callback = null)
		{
			var ret = Create(callback);
			ret.PropPrefab = prefab;
			ret.Position = pos;
			ret.Rotation = rot;
			return ret;
		}
	}

	public class RecycleMonster
		: PooledMessage<RecycleMonster>
	{
		public Monster Monster;

		public static RecycleMonster Create(Monster monster)
		{
			var ret = Create();
			ret.Monster = monster;
			return ret;
		}
	}

	public class RecycleHero
		: PooledMessage<RecycleHero>
	{
		public static new RecycleHero Create()
		{
			var ret = PooledMessage<RecycleHero>.Create();
			return ret;
		}
	}
}
