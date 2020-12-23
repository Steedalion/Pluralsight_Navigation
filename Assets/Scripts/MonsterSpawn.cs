using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Coroutines;

public class MonsterSpawn
	: CoroutineBehaviour
	, IResetable
{
	[SerializeField]
	MonsterTemplate _Template;

	[SerializeField]
	bool _WalkToHero = false;

	[SerializeField]
	CharacterMarker _WalkToTarget;

	[SerializeField]
	float _DetectionDistance = 10.0f;

	[SerializeField]
	bool _AutoRespawn = false;

	[SerializeField]
	float _RespawnDelay = 5.0f;

	// Use this for initialization
	public void Init()
	{
		SetCoroutine(Main());
	}

	public void Dispose()
	{
		CancelCoroutine();
	}

	public bool CanSpawn
	{
		get
		{
			Vector3 screenPoint = CameraManager.Instance.Camera.WorldToViewportPoint(transform.position);
			return (screenPoint.x < 0.0f || screenPoint.x > 1.0f || screenPoint.y < 0.0f || screenPoint.y > 1.0f || screenPoint.z < 0.0f);
		}
	}

	IEnumerable<Instruction> Main()
	{
		while (true)
		{
			// Wait for hero to be close enough
			float distance = float.MaxValue;
			while (distance > _DetectionDistance)
			{
				yield return null;
				distance = Vector3.Distance(transform.position, Game.Instance.Hero.transform.position);
			}

			Monster spawnedMonster = null;
			using (var spawnOne = SpawnOnce(m => spawnedMonster = m).GetEnumerator())
			{
				while (spawnOne.MoveNext())
					yield return spawnOne.Current;
			}

			while (spawnedMonster.Stats.HP > 0.0f)
				yield return null;

			if (!_AutoRespawn)
				break;

			// Wait a second before respawning a monster
			using (var waitASec = Flow.WaitForSeconds(_RespawnDelay).GetEnumerator())
			{
				while (waitASec.MoveNext())
					yield return waitASec.Current;
			}
		}

		// We're done!
	}

	public IEnumerable<Instruction> SpawnOnce(System.Action<Monster> monsterSpawned)
	{
		// Spawn one monster!
		Monster spawnedMonster = null;
		Game.Instance.PushMessage(Messages.SpawnMonster.Create(transform.position, transform.rotation, _Template,
			monster =>
			{
				spawnedMonster = monster;
				if (_WalkToHero)
				{
					monster.SetCoroutine(monster.CreepToHeroThenAttack());
				}
				else if (_WalkToTarget != null)
				{
					monster.SetCoroutine(monster.CreepTo(_WalkToTarget.transform));
				}
				else
				{
					monster.SetCoroutine(monster.AIMain());
				}
			}));

		while (spawnedMonster == null)
			yield return null;

		if (monsterSpawned != null)
			monsterSpawned(spawnedMonster);
	}

	void OnDrawGizmos()
	{
		if (EditorGlobals.Instance.DisplayMarkers)
		{
			// Grab color from layer if available
			Color color = EditorGlobals.Instance.MonsterSpawnDisplayColor;

			// Compute line color from that!
			Color lineColor = color;
#if UNITY_EDITOR
			if (UnityEditor.Selection.activeGameObject == gameObject)
			{
				lineColor.a = EditorGlobals.Instance.CharWireLineAlpha_Selected;
			}
			else
#endif
			{
				lineColor.a = EditorGlobals.Instance.CharWireLineAlpha;
			}

			// Draw the box!
			var mesh = GetComponent<MeshFilter>().sharedMesh;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawWireCube(mesh.bounds.center, mesh.bounds.extents * 1.5f);
			Gizmos.DrawMesh(mesh);
			Gizmos.color = lineColor;
			Gizmos.DrawWireMesh(mesh);

			// Draw a line to the target marker
			if (_WalkToTarget != null)
			{
				Gizmos.matrix = Matrix4x4.identity;
				Gizmos.color = EditorGlobals.Instance.MonsterSpawnDisplayColor;
				Gizmos.DrawLine(transform.TransformPoint(mesh.bounds.center), _WalkToTarget.LinkPoint);
			}
		}
	}
}
