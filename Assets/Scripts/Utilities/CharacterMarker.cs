using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMarker
	: MonoBehaviour
{

	public Vector3 LinkPoint
	{
		get
		{
			var mesh = GetComponent<MeshFilter>().sharedMesh;
			return transform.TransformPoint(mesh.bounds.center);
		}
	}
	void OnDrawGizmos()
	{
		if (EditorGlobals.Instance.DisplayMarkers)
		{
			// Grab color from layer if available
			Color color = EditorGlobals.Instance.CharacterMarkerDisplayColor;

			// Compute line color from that!
			Color lineColor = color;
#if UNITY_EDITOR
			if (UnityEditor.Selection.activeGameObject == gameObject)
			{
				lineColor.a = EditorGlobals.Instance.CharWireLineAlpha_Selected;
			}
			else
#endif
			{
				lineColor.a = EditorGlobals.Instance.CharWireLineAlpha;
			}

			// Draw the box!
			var mesh = GetComponent<MeshFilter>().sharedMesh;
			Gizmos.matrix = transform.localToWorldMatrix;
			Gizmos.color = color;
			Gizmos.DrawMesh(mesh);
			Gizmos.color = lineColor;
			Gizmos.DrawWireMesh(mesh);
		}
	}
}
