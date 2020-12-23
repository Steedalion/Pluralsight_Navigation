using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WorldMouseManager
	: SingletonBehaviour<WorldMouseManager>
	, IManager
{
	// Cached values
	int _WalkableLayerIndex;
	int _NonWalkableLayerIndex;
	int _MonsterLayerIndex;
	int _InteractableLayerIndex;

	int _WalkableLayerMask;
	//int _NonWalkableLayerMask;
	int _MonsterLayerMask;
	int _InteractableLayerMask;

	/// <summary>
	/// The ITarget interface defines something that can be picked by the World Mouse manager
	/// (typically a character, object, etc...)
	/// </summary>
	public interface ITarget
	{
		bool Highlighted { get; set; }
		bool Targetted { get; set; }
	}

	/// <summary>
	/// Base interface for mouse handlers
	/// </summary>
	public interface IMouseHandler
	{
	}

	/// <summary>
	/// Handler used for invalid clicks!
	/// </summary>
	public delegate void InvalidHandler(Vector2 screenPosition);

	/// <summary>
	/// Handler used for navmesh layer
	/// </summary>
	public delegate void NavMeshHandler(Vector3 position);

	/// <summary>
	/// Handler used in input layers for mouse buttons
	/// </summary>
	public delegate void TargetHandler(ITarget target);

	#region Internals

	class NavmeshTarget
		: ITarget
	{
		public bool Highlighted
		{
			get;
			set;
		}

		public bool Targetted
		{
			get;
			set;
		}
	}

	class TargetHandlers
		: IMouseHandler
	{
		public List<ITarget> Monsters;

		public TargetHandler MonsterOnHover;
		public TargetHandler MonsterOffHover;
		public TargetHandler MonsterClick;
	}

	class NavMeshHandlers
		: IMouseHandler
	{
		public NavMeshHandler NavMeshOnHover;
		public NavMeshHandler NavMeshOffHover;
		public NavMeshHandler NavMeshClick;
	}

	class InvalidHandlers
		: IMouseHandler
	{
		public InvalidHandler InvalidOnHover;
		public InvalidHandler InvalidOffHover;
		public InvalidHandler InvalidClick;
	}

	// The list of handlers
	List<TargetHandlers> _MonsterHandlers;
	List<NavMeshHandlers> _NavMeshHandlers;
	List<InvalidHandlers> _InvalidHandlers;

	ITarget _CurrentTarget;
	NavmeshTarget _NavmeshTarget;
	#endregion

	public void Initialize()
	{
		// Register with the Input Manager for clicks!
		InputManager.Instance.SetLayer(InputManager.Layers.WorldMouse, lMouseCallback: OnLMouseEvent, mouseMoveCallback:OnMouseMove);

		_WalkableLayerIndex = LayerMask.NameToLayer(Globals.Instance.Settings.WalkableLayerName);
		//_NonWalkableLayerIndex = LayerMask.NameToLayer(_Settings.NonWalkableLayerName);
		_MonsterLayerIndex = LayerMask.NameToLayer(Globals.Instance.Settings.MonsterLayerName);
		_InteractableLayerIndex = LayerMask.NameToLayer(Globals.Instance.Settings.InteractableLayerName);

		_WalkableLayerMask = 1 << _WalkableLayerIndex;
		//_NonWalkableLayerMask = 1 << _NonWalkableLayerIndex;
		_MonsterLayerMask = 1 << _MonsterLayerIndex;
		_InteractableLayerMask = 1 << _InteractableLayerIndex;

		_MonsterHandlers = new List<TargetHandlers>();
		_NavMeshHandlers = new List<NavMeshHandlers>();
		_InvalidHandlers = new List<InvalidHandlers>();
		_NavmeshTarget = new NavmeshTarget();
	}

	public void Process()
	{
		// Nothing to do for now
	}

	/// <summary>
	/// Register callbacks when the mouse hover or clicks on the navmesh
	/// </summary>
	public IMouseHandler RegisterNavMeshHandlers(
		NavMeshHandler navMeshOnHover = null,
		NavMeshHandler navMeshOffHandler = null,
		NavMeshHandler navMeshClickHandler = null)
	{
		var ret = new NavMeshHandlers()
		{
			NavMeshOnHover = navMeshOnHover,
			NavMeshOffHover = navMeshOffHandler,
			NavMeshClick = navMeshClickHandler
		};
		_NavMeshHandlers.Add(ret);
		return ret;
	}

	/// <summary>
	/// Register callbacks when the mouse hover of clicks on a monster or other target
	/// </summary>
	public IMouseHandler RegisterTargetHandlers(
		List<ITarget> monsters = null,
		TargetHandler monsterOnHover = null,
		TargetHandler monsterOffHover = null,
		TargetHandler monsterClick = null)
	{
		var ret = new TargetHandlers()
		{
			Monsters = monsters,
			MonsterOnHover = monsterOnHover,
			MonsterOffHover = monsterOffHover,
			MonsterClick = monsterClick
		};
		_MonsterHandlers.Add(ret);
		return ret;
	}

	/// <summary>
	/// Register callbacks when the mouse hover or clicks on nothing of interest
	/// </summary>
	public IMouseHandler RegisterInvalidHandlers(
		InvalidHandler invalidOnHover = null,
		InvalidHandler invalidOffHover = null,
		InvalidHandler invalidClick = null)
	{
		var ret = new InvalidHandlers()
		{
			InvalidOnHover = invalidOnHover,
			InvalidOffHover = invalidOffHover,
			InvalidClick = invalidClick
		};
		_InvalidHandlers.Add(ret);
		return ret;
	}

	/// <summary>
	/// Remove a previously registered handler
	/// </summary>
	public void UnregisterHandler(IMouseHandler handler)
	{
		if (handler is TargetHandlers)
			_MonsterHandlers.Remove(handler as TargetHandlers);
		else if (handler is NavMeshHandlers)
			_NavMeshHandlers.Remove(handler as NavMeshHandlers);
		else if (handler is InvalidHandlers)
			_InvalidHandlers.Remove(handler as InvalidHandlers);
		else
		{
			throw new System.NotImplementedException("Invalid handler type " + handler.GetType().Name);
		}
	}

	/// <summary>
	/// We register this method with Input manager to be notified whe nteh mouse move and perform cast against the world
	/// </summary>
	bool OnMouseMove(Vector2 position, bool hasPriority)
	{
		ITarget newTarget = null;
		Vector3 newPosition = Vector3.zero;
		bool ret = false;
		if (hasPriority)
		{
			ret = Cast(position, out newTarget, out newPosition);
		}

		if (_CurrentTarget != newTarget)
		{
			if (_CurrentTarget == _NavmeshTarget)
			{
				// No longer hovering the navmesh, send off hover events!
				foreach (var handlers in _NavMeshHandlers)
				{
					if (handlers.NavMeshOffHover != null)
					{
						handlers.NavMeshOffHover.Invoke(newPosition);
					}
				}
			}
			else if (_CurrentTarget != null)
			{
				// No longer hovering over target or different target
				foreach (var handlers in _MonsterHandlers)
				{
					// Send off hover to handlers caring about cur target!
					if (handlers.MonsterOffHover != null && (handlers.Monsters == null || handlers.Monsters.Contains(_CurrentTarget)))
					{
						handlers.MonsterOffHover.Invoke(_CurrentTarget);
					}
				}
			}
			else
			{
				// Invalid hovering!
				foreach (var handlers in _InvalidHandlers)
				{
					// Send off hover to handlers caring about cur target!
					if (handlers.InvalidOffHover != null)
					{
						handlers.InvalidOffHover.Invoke(position);
					}
				}
			}

			_CurrentTarget = newTarget;

			if (_CurrentTarget == _NavmeshTarget)
			{
				// No longer hovering the navmesh, send off hover events!
				foreach (var handlers in _NavMeshHandlers)
				{
					if (handlers.NavMeshOnHover != null)
					{
						handlers.NavMeshOnHover.Invoke(newPosition);
					}
				}
			}
			else if (_CurrentTarget != null)
			{
				// No longer hovering over target or different target
				foreach (var handlers in _MonsterHandlers)
				{
					// Send off hover to handlers caring about cur target!
					if (handlers.MonsterOnHover != null && (handlers.Monsters == null || handlers.Monsters.Contains(_CurrentTarget)))
					{
						handlers.MonsterOnHover.Invoke(_CurrentTarget);
					}
				}
			}
			else
			{
				// Invalid hovering!
				foreach (var handlers in _InvalidHandlers)
				{
					// Send off hover to handlers caring about cur target!
					if (handlers.InvalidOnHover != null)
					{
						handlers.InvalidOnHover.Invoke(position);
					}
				}
			}
		}

		// Send events!

		return ret;
	}

	/// <summary>
	/// We register this method with the input manager to be notified when the mouse is clicked
	/// so we can check what the user clicked on in the world.
	/// </summary>
	bool OnLMouseEvent(Vector2 position, bool down, bool up)
	{
		bool ret = false;
		if (down)
		{
			ITarget newTarget = null;
			Vector3 newPosition = Vector3.zero;
			ret = Cast(position, out newTarget, out newPosition);
			if (ret)
			{
				if (newTarget == _NavmeshTarget)
				{
					// No longer hovering the navmesh, send off hover events!
					foreach (var handlers in _NavMeshHandlers)
					{
						if (handlers.NavMeshClick != null)
						{
							handlers.NavMeshClick.Invoke(newPosition);
						}
					}
				}
				else if (newTarget != null)
				{
					// No longer hovering over target or different target
					foreach (var handlers in _MonsterHandlers)
					{
						// Send off hover to handlers caring about cur target!
						if (handlers.MonsterClick != null && (handlers.Monsters == null || handlers.Monsters.Contains(_CurrentTarget)))
						{
							handlers.MonsterClick.Invoke(_CurrentTarget);
						}
					}
				}
				else
				{
					throw new System.NotImplementedException("Missing else statement");
				}
			}
			else
			{
				// Invalid click!
				foreach (var handlers in _InvalidHandlers)
				{
					// Send off hover to handlers caring about cur target!
					if (handlers.InvalidClick != null)
					{
						handlers.InvalidClick.Invoke(position);
					}
				}
			}
		}
		return ret;
	}

	/// <summary>
	/// Performs a raycast against the world and filter the result into a valid, registered target!
	/// </summary>
	bool Cast(Vector2 position, out ITarget outTarget, out Vector3 outPosition)
	{
		bool ret = false;
		outTarget = null;
		outPosition = Vector3.zero;

		// Start with a mouse ray
		var ray = CameraManager.Instance.Camera.ScreenPointToRay(position);
		RaycastHit rayHit;
		if (Physics.Raycast(ray, out rayHit, float.MaxValue, _WalkableLayerMask | _MonsterLayerMask | _InteractableLayerMask))
		{
			// We hit something!
			if (rayHit.collider.gameObject.layer == _WalkableLayerIndex)
			{
				// A walkable collider!
				// Check if that point is on the navmesh
				NavMeshHit meshHit;
				if (NavMesh.SamplePosition(rayHit.point, out meshHit, Globals.Instance.Settings.NavMeshDistance, NavMesh.AllAreas))
				{
					outTarget = _NavmeshTarget;
					outPosition = meshHit.position;

					// We ate the event!
					ret = true;
				}
			}
			else if (rayHit.collider.gameObject.layer == _MonsterLayerIndex || rayHit.collider.gameObject.layer == _InteractableLayerIndex)
			{
				// A Monster collider
				// Is there, in fact, a monster or some other target of some kind?
				ITarget target = rayHit.collider.GetComponent<ITarget>();
				if (target != null)
				{
					// Tell the game
					outTarget = target;
					outPosition = rayHit.point;

					// We ate the event!
					ret = true;
				}
			}
		}

		return ret;
	}
}
