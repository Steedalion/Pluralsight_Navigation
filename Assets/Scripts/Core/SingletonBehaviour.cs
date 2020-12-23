using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic base class to implement Global singletons on Monobehaviours
/// </summary>
public class SingletonBehaviour<T>
	: MonoBehaviour
	where T : Object
{
	public static T Instance
	{
		get
		{
			if (_Instance == null)
			{
				// Find the singleton in the scene
				_Instance = GameObject.FindObjectOfType<T>();
			}
			return _Instance;
		}
	}
	static T _Instance; // The actual instance!

	public virtual void OnEnable()
	{
		if (_Instance == null)
		{
			_Instance = this as T;
		}
		else
		{
			Debug.LogError("More than one " + GetType().ToString() + " in scene!");
		}
	}

	public virtual void OnDisable()
	{
		if (_Instance == this)
		{
			_Instance = null;
		}
	}
}
