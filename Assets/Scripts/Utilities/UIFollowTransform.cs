using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIFollowTransform
: MonoBehaviour
{
	public Transform Target
	{
		get;
		set;
	}

	RectTransform _ThisTransform;

	// Use this for initialization
	void Awake()
	{
		_ThisTransform = GetComponent<RectTransform>();
	}

	// Update is called once per frame
	void LateUpdate()
	{
		// Get screen pos of target
		Vector2 screenPos;
		if (UIManager.Instance.WorldPointToLocalPoint(Target.position, out screenPos))
		{
			_ThisTransform.anchoredPosition = screenPos;
		}
	}
}
