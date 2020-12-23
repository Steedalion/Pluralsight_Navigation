using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Messages;
using Coroutines;

/// <summary>
/// Defines the interface for an object that can be paused!
/// </summary>
public interface IPausable
{
	void Pause();
	void Resume();
}


public partial class Game
	: SingletonBehaviour<Game>
{
	[Header("Scene References")]
	[SerializeField]
	Transform _SpawnMarker;

	[SerializeField]
	Doorway _Doorway;

	#region Properties
	// The Hero, once instantiated!
	public Hero Hero
	{
		get { return _ObjectManager != null ? _ObjectManager.Hero : null; }
	}

	public bool Paused
	{
		get;
		private set;
	}
	#endregion

	// Registered handlers
	WorldMouseManager.IMouseHandler _NavmeshHandler;
	WorldMouseManager.IMouseHandler _MonsterHandler;
	WorldMouseManager.IMouseHandler _InvalidHandler;
	WorldMouseManager.IMouseHandler _DoorwayHandler;

	InputManager.ILayer _EscapeHandlerLayer;

	/// <summary>
	/// Initializes all the logic of the game
	/// This method in essence ties all the modules together!
	/// </summary>
	void InitializeGameLogic()
	{
		// Make sure we have a spawn marker
		Debug.Assert(_SpawnMarker != null);

		GameCoroutine = new CoroutineSite();

		// Initialize the game-specific message handlers!

		AddMessageHandler<MouseClickNavmesh>(msg =>
		{
			_FXManager.ClickAknowledge(msg.Destination);
			_AudioManager.PlayAcknowledge();
			PushMessage(Messages.RunTo.Create(Hero, msg.Destination, 0.0f, null));
		});

		AddMessageHandler<MouseClickMonster>(msg =>
		{
			_AudioManager.PlayAcknowledge();
			PushMessage(Messages.Attack.Create(Hero, msg.Monster, null));
		});

		AddMessageHandler<MouseClickNothing>(msg =>
		{
			_AudioManager.PlayInvalid();
		});

		AddMessageHandler<ApplyDamage>(msg =>
		{
			var character = msg.Target as Character;

			float prevHP = character.Stats.HP;
			character.ApplyDamage(msg.Damage);

			// Play impact fx
			Vector3 center = (msg.Source.transform.position + msg.Target.transform.position) * 0.5f + Vector3.up * 1.0f;
			_FXManager.SwordImpact(center, Quaternion.identity);

			if (prevHP > 0.0f && character.Stats.HP <= 0.0f)
			{
				Game.Instance.PushMessage(Messages.Die.Create(character, null));
			}
		});

		AddMessageHandler<SpawnHero>(msg =>
		{
			// Spawn the hero and set member variable
			var hero = _ObjectManager.InstantiateHero(msg.Position, msg.Rotation, msg.Template.Prefab);
			hero.Initialize(ref msg.Template.Stats);
			_UIManager.CreateHeroUI();

			var propCon = hero.GetComponent<PropController>();
			if (Globals.Instance.Settings.HeroTemplate.Sword != null && propCon != null)
			{
				ProcessMessage(SpawnProp.Create(Vector3.zero, Quaternion.identity, msg.Template.Sword,
					sword =>
					{
						ProcessMessage(AttachProp.Create(hero, sword, null));
					}));
			}

			if (msg.Callback != null)
			{
				msg.Callback.Invoke(hero);
			}
		});

		AddMessageHandler<SpawnMonster>(msg =>
		{
			var monster = _ObjectManager.InstantiateMonster(msg.Position, msg.Rotation, msg.Template);
			_UIManager.CreateMonsterUI(monster);

			var propCon = monster.GetComponent<PropController>();
			if (Globals.Instance.Settings.HeroTemplate.Sword != null && propCon != null)
			{
				ProcessMessage(SpawnProp.Create(Vector3.zero, Quaternion.identity, msg.Template.Sword,
					hammer =>
					{
						ProcessMessage(AttachProp.Create(monster, hammer, null));
					}));
			}

			if (msg.Callback != null)
			{
				msg.Callback.Invoke(monster);
			}
		});

		AddMessageHandler<RecycleMonster>(msg =>
		{
			_UIManager.DestroyMonsterUI(msg.Monster);
			_ObjectManager.RecycleMonster(msg.Monster);
		});

		AddMessageHandler<RecycleHero>(msg =>
		{
			_UIManager.DestroyHeroUI();
			_ObjectManager.RecycleHero();
		});

		AddMessageHandler<SpawnProp>(msg =>
		{
			// Spawn the hero and set member variable
			var sword = _ObjectManager.InstantiateProp(msg.Position, msg.Rotation, msg.PropPrefab);
			if (msg.Callback != null)
			{
				msg.Callback.Invoke(sword);
			}
		});

		AddMessageHandler<AttachProp>(msg =>
		{
			var propCon = msg.Character.GetComponent<PropController>();
			if (propCon != null)
			{
				propCon.AttachProp(msg.Prop);
			}
		});

		AddMessageHandler<MonsterAggro>(msg =>
		{
			_CombatManager.MonsterAggro(msg.Monster);
		});

		AddMessageHandler<Die>(msg =>
		{
			// Notify the combat manager
			if (msg.Character is Monster)
			{
				var monster = msg.Character as Monster;
				_CombatManager.MonsterDead(msg.Character as Monster);
				_UIManager.UpdateKillCount();
				monster.SetCoroutine(monster.Die(() => PushMessage(RecycleMonster.Create(monster))));
			}
			else if (msg.Character is Hero)
			{
				GameCoroutine.SetCoroutine(PlayerDied());
			}
		});

		AddMessageHandler<CombatStarted>(msg =>
		{
			_AudioManager.OnCombatStart();
		});

		AddMessageHandler<CombatEnded>(msg =>
		{
			_AudioManager.OnCombatEnd();
		});

		AddMessageHandler<RestartGame>(msg =>
		{
			GameCoroutine.SetCoroutine(RestartGameCr());
		});

		AddMessageHandler<PauseGame>(msg =>
		{
			GameCoroutine.SetCoroutine(PauseGame());
		});

		AddMessageHandler<UnpauseGame>(msg =>
		{
			GameCoroutine.SetCoroutine(ResumeGame());
		});

		AddMessageHandler<QuitGame>(msg =>
		{
			GameCoroutine.SetCoroutine(QuitGame());
		});

		// Start the Game!!
		GameCoroutine.SetCoroutine(StartGameCr());
	}

	IEnumerable<Instruction> RestartGameCr()
	{
		using (var endGame = EndGameCr().GetEnumerator())
		{
			while (endGame.MoveNext())
				yield return endGame.Current;
		}

		using (var unpause = ResumeGame().GetEnumerator())
		{
			while (unpause.MoveNext())
				yield return unpause.Current;
		}

		using (var startGame = StartGameCr().GetEnumerator())
		{
			while (startGame.MoveNext())
				yield return startGame.Current;
		}
	}

	IEnumerable<Instruction> EndGameCr()
	{
		_UIManager.ShowFader();

		// Unregister the escape key
		_InputManager.PopLayer(_EscapeHandlerLayer);

		using (var wait = Flow.WaitForSeconds(0.25f).GetEnumerator())
		{
			while (wait.MoveNext())
				yield return wait.Current;
		}

		_UIManager.HideGameOver();

		// Despawn everyone
		foreach (var monster in _ObjectManager.Monsters)
		{
			_CombatManager.MonsterDead(monster);
			PushMessage(RecycleMonster.Create(monster));
		}
		PushMessage(RecycleHero.Create());

		_ObjectManager.Reset();

		// Unregister for clicks on navmesh and monsters!
		_InteractionManager.UnregisterHandler(_NavmeshHandler);
		_InteractionManager.UnregisterHandler(_MonsterHandler);
		_InteractionManager.UnregisterHandler(_InvalidHandler);
		_InteractionManager.UnregisterHandler(_DoorwayHandler);

		RemoveHeroMessageHandlers();

		foreach (var resetable in _AllResetables)
		{
			resetable.Dispose();
		}
	}

	IEnumerable<Instruction> StartGameCr()
	{
		foreach (var resetable in _AllResetables)
		{
			resetable.Init();
		}

		// Create the hero!
		PushMessage(SpawnHero.Create(_SpawnMarker.position, _SpawnMarker.rotation, Globals.Instance.Settings.HeroTemplate,
			hero =>
			{
				_CameraManager.SnapToTarget();
				AddHeroMessageHandlers();
			}));

		using (var wait = Flow.WaitForSeconds(0.25f).GetEnumerator())
		{
			while (wait.MoveNext())
				yield return wait.Current;
		}

		_CombatManager.Reset();
		_UIManager.UpdateKillCount();
		_UIManager.HideFader();
		_AudioManager.FadeIn();

		// Register for clicks on navmesh and monsters!
		_NavmeshHandler = _InteractionManager.RegisterNavMeshHandlers(
			navMeshClickHandler: pos => PushMessage(MouseClickNavmesh.Create(pos)),
			navMeshOnHover: pos => _CursorManager.OnNavMeshHover());

		_MonsterHandler = _InteractionManager.RegisterTargetHandlers(
			monsterClick: monster =>
			{
				if (monster is Monster)
				{
					PushMessage(MouseClickMonster.Create(monster as Monster));
				}
			},
			monsterOnHover: monster =>
			{
				if (monster is Monster)
				{
					_CursorManager.OnMonsterHover();
					monster.Highlighted = true;
				}
			},
			monsterOffHover: monster =>
			{
				if (monster is Monster)
				{
					monster.Highlighted = false;
				}
			});

		_InvalidHandler = _InteractionManager.RegisterInvalidHandlers(
			invalidClick: pos => PushMessage(MouseClickNothing.Create()),
			invalidOnHover: pos => _CursorManager.OnNothingHover());

		// Register for clicks on the doorway
		var doorwayList = new List<WorldMouseManager.ITarget>();
		doorwayList.Add(_Doorway);
		_DoorwayHandler = _InteractionManager.RegisterTargetHandlers(
			 doorwayList,
			 monsterClick: pos =>
			 {
				 if (!_CombatManager.IsInCombat)
				 {
					 PushMessage(MouseClickDoorway.Create(_Doorway));
				 }
			 },
			 monsterOnHover: pos =>
			 {
				 if (!_CombatManager.IsInCombat)
				 {
					 _CursorManager.OnDoorwayHover();
				 }
			 });

		// Register the escape key
		List<KeyCode> escape = new List<KeyCode>();
		escape.Add(KeyCode.Escape);
		_EscapeHandlerLayer = _InputManager.SetLayer(
			InputManager.Layers.Escape,
			keys: escape,
			keyCallback: OnEscapeKey);
	}

	bool OnEscapeKey(KeyCode key, bool down, bool up)
	{
		if (down)
		{
			// Show the pause menu
			if (Paused)
			{
				PushMessage(Messages.UnpauseGame.Create());
			}
			else
			{
				PushMessage(Messages.PauseGame.Create());
			}
		}
		return true;
	}

	IEnumerable<Instruction> QuitGame()
	{
		_UIManager.HideGameOver();
		_UIManager.HideGamePaused();
		_UIManager.ShowFader();
		_AudioManager.FadeOut();

		using (var wait = Flow.WaitForSeconds(0.25f).GetEnumerator())
		{
			while (wait.MoveNext())
				yield return wait.Current;
		}

		Application.Quit();
	}

	IEnumerable<Instruction> PlayerDied()
	{
		RemoveHeroMessageHandlers();

		// Have the player play their death anim
		bool playerDeathAnimFinished = false;
		Hero.SetCoroutine(Hero.Die(() => playerDeathAnimFinished = true));

		// Wait for it to finish
		while (!playerDeathAnimFinished)
		{
			yield return null;
		}

		// Show the game over screen
		_UIManager.ShowFader();
		_UIManager.ShowGameOver();
	}

	IEnumerable<Instruction> PauseGame()
	{
		foreach (var pauseable in _AllPauseables)
		{
			pauseable.Pause();
		}

		Paused = true;
		_UIManager.ShowGamePaused();
		yield break;
	}

	IEnumerable<Instruction> ResumeGame()
	{
		_UIManager.HideGamePaused();
		Paused = false;

		foreach (var pauseable in _AllPauseables)
		{
			pauseable.Resume();
		}

		yield break;
	}

	void AddHeroMessageHandlers()
	{
		AddMessageHandler<RunTo>(msg =>
		{
			msg.Hero.SetCoroutine(msg.Hero.RunTo(msg.Destination, msg.DistanceFromGoal, null));
		});

		AddMessageHandler<Attack>(msg =>
		{
			msg.Hero.SetCoroutine(msg.Hero.AutoAttack(msg.Target, null));
		});

		AddMessageHandler<MouseClickDoorway>(msg =>
		{
			Hero.SetCoroutine(Hero.WalkThroughDoorway(msg.Doorway));
		});

	}

	void RemoveHeroMessageHandlers()
	{
		RemoveMessageHandler<RunTo>();
		RemoveMessageHandler<Attack>();
		RemoveMessageHandler<MouseClickDoorway>();
	}
}
