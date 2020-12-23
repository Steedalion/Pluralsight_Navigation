using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Messages
{
	public class MouseHoverNavMesh
		: PooledMessage<MouseHoverNavMesh>
	{
		public Vector3 Destination;

		public static MouseHoverNavMesh Create(Vector3 destination)
		{
			var ret = Create();
			ret.Destination = destination;
			return ret;
		}
	}

	public class MouseHoverMonster
		: PooledMessage<MouseHoverMonster>
	{
		public Monster Monster;
		public static MouseHoverMonster Create(Monster monster)
		{
			var ret = Create();
			ret.Monster = monster;
			return ret;
		}
	}

	public class MouseHoverNothing
		: PooledMessage<MouseHoverNothing>
	{
	}

	public class MouseClickNavmesh
		: PooledMessage<MouseClickNavmesh>
	{
		public Vector3 Destination;
		public static MouseClickNavmesh Create(Vector3 destination)
		{
			var ret = Create();
			ret.Destination = destination;
			return ret;
		}
	}

	public class MouseClickMonster
		: PooledMessage<MouseClickMonster>
	{
		public Monster Monster;
		public static MouseClickMonster Create(Monster monster)
		{
			var ret = Create();
			ret.Monster = monster;
			return ret;
		}
	}

	public class MouseClickNothing
		: PooledMessage<MouseClickNothing>
	{
		public static new MouseClickNothing Create()
		{
			return PooledMessage<MouseClickNothing>.Create();
		}
	}

	public class MouseClickDoorway
		: PooledMessage<MouseClickDoorway>
	{
		public Doorway Doorway;

		public static MouseClickDoorway Create(Doorway doorway)
		{
			var ret = Create();
			ret.Doorway = doorway;
			return ret;
		}
	}

	public class ApplyDamage
		: PooledMessage<ApplyDamage>
	{
		public Character Source;
		public Character Target;
		public float Damage;

		public static ApplyDamage Create(Character target, Character source, float damage)
		{
			var ret = Create();
			ret.Source = source;
			ret.Target = target;
			ret.Damage = damage;
			return ret;
		}
	}

	public class SpawnerReady
		: PooledMessage<SpawnerReady>
	{
		public MonsterSpawn Spawn;

		public static SpawnerReady Create(MonsterSpawn spawn)
		{
			var ret = Create();
			ret.Spawn = spawn;
			return ret;
		}
	}

	public class CombatStarted
		: PooledMessage<CombatStarted>
	{
		public static new CombatStarted Create()
		{
			return PooledMessage<CombatStarted>.Create();
		}
	}

	public class CombatEnded
		: PooledMessage<CombatEnded>
	{
		public static new CombatEnded Create()
		{
			return PooledMessage<CombatEnded>.Create();
		}
	}

	public class QuitGame
		: PooledMessage<QuitGame>
	{
		public static new QuitGame Create()
		{
			return PooledMessage<QuitGame>.Create();
		}
	}


	public class RestartGame
		:PooledMessage<RestartGame>
	{
		public static new RestartGame Create()
		{
			return PooledMessage<RestartGame>.Create();
		}
	}

	public class UnpauseGame
		: PooledMessage<UnpauseGame>
	{
		public static new UnpauseGame Create()
		{
			return PooledMessage<UnpauseGame>.Create();
		}
	}

	public class PauseGame
		: PooledMessage<PauseGame>
	{
		public static new PauseGame Create()
		{
			return PooledMessage<PauseGame>.Create();
		}
	}

}
