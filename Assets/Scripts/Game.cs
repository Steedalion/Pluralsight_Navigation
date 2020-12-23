#define WARN_IF_MESSAGE_NOT_HANDLED

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Messages;
using Coroutines;


/// <summary>
/// Controls all other managers in the game, and the order in which things happen every frame!
/// </summary>
public partial class Game
	: SingletonBehaviour<Game>
{
	[SerializeField]
	Transform _ResetableRoot;

	// The Managers!
	InputManager _InputManager;
	WorldMouseManager _InteractionManager;
	CameraManager _CameraManager;
	ObjectManager _ObjectManager;
	CursorManager _CursorManager;
	AudioManager _AudioManager;
	UIManager _UIManager;
	FXManager _FXManager;
	CombatManager _CombatManager;

	// Queued messages!
	Queue<IMessage> _Actions;

	// Matches each type of message to a handler
	Dictionary<int, System.Action<IMessage>> _MessageMap;

	CoroutineSite GameCoroutine;

	List<IResetable> _AllResetables;
	HashSet<IPausable> _AllPauseables;

	public void PushMessage(IMessage message)
	{
		_Actions.Enqueue(message);
	}

	public void RegisterPauseable(IPausable pausable)
	{
		_AllPauseables.Add(pausable);
	}

	public void UnregisterPauseable(IPausable pausable)
	{
		_AllPauseables.Remove(pausable);
	}

	void Awake()
	{
		_AllPauseables = new HashSet<IPausable>();
	}

	// Use this for initialization
	void Start ()
	{
		_Actions = new Queue<IMessage>();
		_MessageMap = new Dictionary<int, System.Action<IMessage>>();
		_AllResetables = new List<IResetable>(_ResetableRoot.GetComponentsInChildren<IResetable>());

		// Find all the manager components stored on this gameobject
		_InputManager = GetComponent<InputManager>();
		_InteractionManager = GetComponent<WorldMouseManager>();
		_CameraManager = GetComponent<CameraManager>();
		_ObjectManager = GetComponent<ObjectManager>();
		_CursorManager = GetComponent<CursorManager>();
		_AudioManager = GetComponent<AudioManager>();
		_UIManager = GetComponent<UIManager>();
		_FXManager = GetComponent<FXManager>();
		_CombatManager = GetComponent<CombatManager>();

		// Make sure we have all the components we expect then!
		Debug.Assert(_InputManager != null);
		Debug.Assert(_InteractionManager != null);
		Debug.Assert(_CameraManager != null);
		Debug.Assert(_ObjectManager != null);
		Debug.Assert(_CursorManager != null);
		Debug.Assert(_AudioManager != null);
		Debug.Assert(_UIManager != null);
		Debug.Assert(_FXManager != null);
		Debug.Assert(_CombatManager != null);

		// Initialize the managers in the correct order
		_UIManager.Initialize();
		_ObjectManager.Initialize();
		_CameraManager.Initialize();
		_InputManager.Initialize();
		_InteractionManager.Initialize();
		_CursorManager.Initialize();
		_AudioManager.Initialize();
		_CombatManager.Initialize();

		// Create object, register handlers, etc...
		InitializeGameLogic();
	}

	// Update is called once per frame
	void Update ()
	{
		GameCoroutine.Update();

		// Process our managers
		Profiler.BeginSample("Input Manager");
		_InputManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("Interaction Manager");
		_InteractionManager.Process();
		Profiler.EndSample();

		// Process the entire action queue
		Profiler.BeginSample("Process Messages");
		ProcessMessageLoop();
		Profiler.EndSample();

		Profiler.BeginSample("Cursor Manager");
		_CursorManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("Object Manager");
		_ObjectManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("UI Manager");
		_UIManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("Camera Manager");
		_CameraManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("Audio Manager");
		_AudioManager.Process();
		Profiler.EndSample();
		Profiler.BeginSample("Combat Manager");
		_CombatManager.Process();
		Profiler.EndSample();
	}

	void LateUpdate()
	{
		GameCoroutine.LateUpdate();
	}

	void OnAnimatorMove()
	{
		GameCoroutine.OnAnimatorMove();
	}

	void AddMessageHandler<TMessage>(System.Action<TMessage> handler)
		where TMessage : class, IMessage, new()
	{
		TMessage dummy = new TMessage();
		if (_MessageMap.ContainsKey(dummy.Id))
		{
			Debug.Log("Message " + dummy.Name + " already registered");
		}
		else
		{
			_MessageMap.Add(dummy.Id, (msg) => handler(msg as TMessage));
		}
	}

	void RemoveMessageHandler<TMessage>()
		where TMessage : class, IMessage, new()
	{
		TMessage dummy = new TMessage();
		if (!_MessageMap.Remove(dummy.Id))
		{
			Debug.Log("Message handler " + dummy.Id + " not in handlers");
		}
	}

	void ProcessMessageLoop()
	{
		while (_Actions.Count > 0)
		{
			var message = _Actions.Dequeue();
			ProcessMessage(message);
		}
	}

	void ProcessMessage(IMessage message)
	{
		Profiler.BeginSample(message.Name);
		System.Action<IMessage> handler = null;
		if (_MessageMap.TryGetValue(message.Id, out handler))
		{
			handler.Invoke(message);
		}
		else
		{
#if WARN_IF_MESSAGE_NOT_HANDLED
			Debug.LogWarning("No handler for message " + message.Name);
#endif
		}

		Profiler.EndSample();

		// We're done with the action, return it to its pool!
		message.Dispose();
	}
}
