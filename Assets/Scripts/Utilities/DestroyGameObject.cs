using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGameObject
	: MonoBehaviour
{
	[SerializeField]
	bool _AutoDestroy;

	[SerializeField]
	float _AutoDestroyTimer = 1.0f;

	public void Destroy()
	{
		GameObject.Destroy(gameObject);
	}

	void Start()
	{
		if (_AutoDestroy)
		{
			GameObject.Destroy(gameObject, _AutoDestroyTimer);
		}
		// Else we're expecting someone to call Destroy()
	}
}
