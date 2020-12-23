using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FXManager
	: MonoBehaviour
	, IManager
{
	public void ClickAknowledge(Vector3 position)
	{
		GameObject.Instantiate(Globals.Instance.Settings.ClickAknowledgePrefab, position, Quaternion.identity);
	}

	public void SwordImpact(Vector3 position, Quaternion rotation)
	{
		GameObject.Instantiate(Globals.Instance.Settings.SwordImpactPrefab, position, rotation);
	}

	public void Initialize()
	{
	}

	public void Process()
	{
	}
}
