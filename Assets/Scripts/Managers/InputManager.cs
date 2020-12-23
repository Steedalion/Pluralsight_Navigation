using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
	: SingletonBehaviour<InputManager>
	, IManager
{
	[Header("Scene References")]
	[SerializeField]
	EventSystem _EventSystem;

	/// <summary>
	/// Handler used in input layers for keyboard keys
	/// </summary>
	/// <param name="key">The key that triggered</param>
	/// <param name="down">Whether the key was just pressed</param>
	/// <param name="up">Whether the key was just released</param>
	/// <returns>returns true if the Layer (client) processed the event, false
	/// if it should be passed on to the next layer</returns>
	public delegate bool KeyCodeHandler(KeyCode key, bool down, bool up);
	/// <summary>
	/// Handler used in input layers for mouse buttons
	/// </summary>
	/// <param name="position">The mouse position</param>
	/// <param name="down">Whether the mouse buton was just pressed</param>
	/// <param name="up">Whether the mouse button was just released</param>
	/// <returns>returns true if the Layer (client) processed the event, false
	/// if it should be passed on to the next layer</returns>
	public delegate bool MouseButtonHandler(Vector2 position, bool down, bool up);
	/// <summary>
	/// Handler used in input layers for mouse movement
	/// </summary>
	/// <param name="position">The mouse position</param>
	/// <param name="hasPriority">Whether this handler currently has priority</param>
	/// <returns>returns true if the Layer (client) processed the event, false
	/// if it should be passed on to the next layer</returns>
	public delegate bool MouseMoveHandler(Vector2 position, bool hasPriority);

	// Internal data
	Vector3 _LastMousePos;

	public enum Layers
	{
		WorldMouse = 0,
		ScreenUI,
		Escape,
		Count
	}

	public interface ILayer
	{

	}

	/// <summary>
	/// An input layer can set as many callbacks as it wants to be notified of keyboard/mouse events
	/// </summary>
	class Layer
		: ILayer
	{
		public Layers LayerIndex;
		public List<KeyCode> Keys;
		public KeyCodeHandler KeyCallback;
		public MouseButtonHandler LMouseCallback;
		public MouseButtonHandler RMouseCallback;
		public MouseMoveHandler MouseMoveCallback;
	}

	Layer [] _Layers;

	public void Initialize()
	{
		_Layers = new Layer[(int)Layers.Count];
	}

	/// <summary>
	/// Push a new input layer onto the stack, so users can be notified of events
	/// </summary>
	public ILayer SetLayer(
		Layers layer,
		List<KeyCode> keys = null,
		KeyCodeHandler keyCallback = null,
		MouseButtonHandler lMouseCallback = null,
		MouseButtonHandler rMouseCallback = null,
		MouseMoveHandler mouseMoveCallback = null)
	{
		Layer ret = new Layer()
		{
			LayerIndex = layer,
			Keys = keys,
			KeyCallback = keyCallback,
			LMouseCallback = lMouseCallback,
			RMouseCallback = rMouseCallback,
			MouseMoveCallback = mouseMoveCallback
		};

		_Layers[(int)layer] = ret;

		return ret;
	}

	/// <summary>
	/// Removes a layer from the stack
	/// </summary>
	public void PopLayer(ILayer layer)
	{
		_Layers[(int)((layer as Layer).LayerIndex)] = null;
	}

	public void Process()
	{
		if (Input.anyKey)
		{
			// Start with keyboard keys
			foreach (var layer in _Layers)
			{
				if (layer != null && layer.KeyCallback != null)
				{
					if (layer.Keys != null)
					{
						foreach (var key in layer.Keys)
						{
							bool down = Input.GetKeyDown(key);
							bool up = Input.GetKeyUp(key);
							if (up || down)
							{
								if (layer.KeyCallback(key, down, up))
								{
									// The layer processed the event, so we don't need to pass it down further
									break;
								}
							}
						}
					}
				}
			}

			// Then do left and right mouse!
			bool lmouseDown = Input.GetMouseButtonDown(0);
			bool lmouseUp = Input.GetMouseButtonUp(0);
			if (lmouseDown || lmouseUp)
			{
				// Go through our layers and check, in reverse order because the newest layer has highest priority
				for (int i = _Layers.Length - 1; i >= 0; --i)
				{
					var layer = _Layers[i];
					if (layer != null && layer.LMouseCallback != null)
					{
						if (layer.LMouseCallback.Invoke(Input.mousePosition, lmouseDown, lmouseUp))
						{
							// The layer processed the event, so we don't need to pass it down further
							break;
						}
					}
				}
			}

			// Then do left and right mouse!
			bool rmouseDown = Input.GetMouseButtonDown(1);
			bool rmouseUp = Input.GetMouseButtonUp(1);
			if (rmouseDown || rmouseUp)
			{
				// Go through our layers and check, in reverse order because the newest layer has highest priority
				for (int i = _Layers.Length - 1; i >= 0; --i)
				{
					var layer = _Layers[i];
					if (layer != null && layer.RMouseCallback != null)
					{
						if (layer.RMouseCallback.Invoke(Input.mousePosition, rmouseDown, rmouseUp))
						{
							// The layer processed the event, so we don't need to pass it down further
							break;
						}
					}
				}
			}
		}

		if (Input.mousePosition != _LastMousePos)
		{
			_LastMousePos = Input.mousePosition;
			bool hasPriority = true;
			// Go through our layers and check, in reverse order because the newest layer has highest priority
			for (int i = _Layers.Length - 1; i >= 0; --i)
			{
				var layer = _Layers[i];
				if (layer != null && layer.MouseMoveCallback != null)
				{
					if (layer.MouseMoveCallback.Invoke(Input.mousePosition, hasPriority))
					{
						// The layer processed the event, so further layers don't have priority anymore
						hasPriority = false;
					}
				}
			}
		}
	}
}
