using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coroutines;

namespace Messages
{
	/// <summary>
	/// Base interface for all character messages so they are easy to filter out
	/// </summary>
	public interface ICharacterMessage 
		: IMessage
	{
		Character Character { get; set; }
		System.Action Callback { get; set; }
	}

	/// <summary>
	/// Helper class to create pooled character messages without having to reimplement the interface every time
	/// </summary>
	public class PooledCharacterMessage<T>
		: PooledMessage<T>
		, ICharacterMessage
		where T : class, ICharacterMessage, new()
	{
		public Character Character { get; set; }
		public System.Action Callback { get; set; }

		public static T Create(Character character, System.Action callback)
		{
			var ret = Create();
			ret.Character = character;
			ret.Callback = callback;
			return ret;
		}
	}

	public class AttachProp
		: PooledCharacterMessage<AttachProp>
	{
		public Prop Prop;
		public static AttachProp Create(Character character, Prop prop, System.Action callback)
		{
			var ret = Create(character, callback);
			ret.Prop = prop;
			return ret;
		}
	}

	public class PooledHeroMessage<T>
		: PooledCharacterMessage<T>
		where T : class, ICharacterMessage, new()
	{
		public Hero Hero
		{
			get { return Character as Hero; }
		}
		public static T Create(Hero hero, System.Action callback)
		{
			var ret = PooledCharacterMessage<T>.Create(hero, callback);
			return ret;
		}
	}

	public class RunTo
		: PooledHeroMessage<RunTo>
	{
		public Vector3 Destination;
		public float DistanceFromGoal;

		public static RunTo Create(Hero hero, Vector3 destination, float distanceFromGoal, System.Action callback)
		{
			var ret = Create(hero, callback);
			ret.Destination = destination;
			ret.DistanceFromGoal = distanceFromGoal;
			return ret;
		}
	}

	public class Attack
		: PooledHeroMessage<Attack>
	{
		public Character Target;

		public static Attack Create(Hero hero, Character target, System.Action callback)
		{
			var ret = Create(hero, callback);
			ret.Target = target;
			return ret;
		}
	}

	// Temp message!
	public class MonsterAggro
		: PooledMessage<MonsterAggro>
	{
		public Monster Monster;
		public Character Target;
		public static MonsterAggro Create(Monster monster, Character target)
		{
			var ret = Create();
			ret.Monster = monster;
			ret.Target = target;
			return ret;
		}
	}

	public class Die
		: PooledCharacterMessage<Die>
	{
		// We'll add some stuff here!
	}
}
