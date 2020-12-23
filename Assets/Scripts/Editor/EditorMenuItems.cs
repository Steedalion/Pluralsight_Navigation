using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class EditorMenuItems
{
	[MenuItem("SAS/Bake Navmesh")]
	static void GoBakeNavmesh()
	{
		BakeNavmesh.Bake();
	}

	[MenuItem("SAS/ShowHide Gizmos")]
	static void ShowHideGizmos()
	{
		EditorGlobals.Instance.DisplayMarkers = !EditorGlobals.Instance.DisplayMarkers;
		SceneView.RepaintAll();
	}
}
