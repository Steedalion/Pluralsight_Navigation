using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small Utility component that renders a box using Gizmos so that it only
/// displays when selected in the editor view, not in game
/// </summary>
public class NavmeshCleanupBox
	: MonoBehaviour
{
	void OnDrawGizmos()
	{
		if (EditorGlobals.Instance.DisplayMarkers)
		{
			// Grab color from layer if available
			Color color = EditorGlobals.Instance.CleanupNavmeshCollisionBoxColor;
			Color lineColor = color;
#if UNITY_EDITOR
			if (UnityEditor.Selection.activeGameObject == gameObject)
			{
				lineColor.a = EditorGlobals.Instance.BoxWireLineAlpha_Selected;
			}
			else
#endif
			{
				lineColor.a = EditorGlobals.Instance.BoxWireLineAlpha;
			}

			// Draw the box!
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
			Gizmos.color = lineColor;
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
	}
}
