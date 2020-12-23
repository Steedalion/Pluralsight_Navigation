using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager
	: SingletonBehaviour<CameraManager>
	, IManager
{
	[Header("Scene References")]
	[SerializeField]
	Camera _Camera;

	Vector3 _CameraVelocity;

	#region Properties
	public Transform FocusTransform
	{
		get { return Game.Instance.Hero != null ? Game.Instance.Hero.CameraFocus : null; }
	}

	public Camera Camera
	{
		get { return _Camera; }
	}
	#endregion

	public void Initialize()
	{
		// Nothing for now
	}

	public void Process()
	{
		// Update the camera if possible
		if (FocusTransform != null)
		{
			// Update the position of the Camera to match the hero!
			UpdateCamera(FocusTransform.position, Globals.Instance.Settings.SmoothDamp);
		}
	}

	public void SnapToTarget()
	{
		// Update the camera if possible
		if (FocusTransform != null)
		{
			// Update the position of the Camera to match the hero!
			UpdateCamera(FocusTransform.position, 0.0f);
		}
	}

	public void UpdateCamera(Vector3 point, float smoothDamp)
	{
		// Offset the camera first
		Vector3 targetPosition = point + Globals.Instance.Settings.CameraOffset;
		_Camera.transform.position = Vector3.SmoothDamp(_Camera.transform.position, targetPosition, ref _CameraVelocity, smoothDamp);

		// And then make it look back at the hero!
		Quaternion rot = Quaternion.LookRotation(-Globals.Instance.Settings.CameraOffset, Vector3.up);
		_Camera.transform.rotation = rot;
	}
}
