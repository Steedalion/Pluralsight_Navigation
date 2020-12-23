using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraManager))]
public class CameraManagerEditor : Editor
{
	public void OnSceneGUI()
	{
		SerializedObject globals = new SerializedObject(Globals.Instance);
		var settings = globals.FindProperty("_Settings").objectReferenceValue as GameSettings;
		if (settings != null)
		{
			EditorGUI.BeginChangeCheck();

			// Find a reference point
			Vector3 refPos = FindRefPos();
			Vector3 offset = settings.CameraOffset;
			Vector3 pos = refPos + offset;

			// Make a position handle that always points to the focus point!
			Quaternion rot = Quaternion.LookRotation(new Vector3(-offset.x, 0.0f, -offset.z), Vector3.up);
			pos = Handles.PositionHandle(pos, rot);

			if (EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(target, "Free Move Camera Offset");
				settings.CameraOffset = pos - refPos;
				CameraManager.Instance.UpdateCamera(refPos, 0.0f);
			}
		}
	}

	public override void OnInspectorGUI()
	{
		EditorGUI.BeginChangeCheck();
		base.OnInspectorGUI();
		if (EditorGUI.EndChangeCheck())
		{
			CameraManager t = (target as CameraManager);
			Vector3 refPos = FindRefPos();
			t.UpdateCamera(refPos, 0.0f);
		}
	}

	Vector3 FindRefPos()
	{
		CameraManager t = (target as CameraManager);
		Vector3 refPos = Vector3.zero;
		if (t.FocusTransform != null)
		{
			refPos = t.FocusTransform.position;
		}
		else
		{
			SerializedObject gameObj = new SerializedObject(Game.Instance);
			var marker = gameObj.FindProperty("_SpawnMarker");
			refPos = (marker.objectReferenceValue as Transform).position;
		}
		return refPos;
	}
}
