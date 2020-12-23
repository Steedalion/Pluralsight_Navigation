using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Doorway
	: MonoBehaviour
	, WorldMouseManager.ITarget
{
	[SerializeField]
	CharacterMarker _StartMarker;

	[SerializeField]
	CharacterMarker _EndMarker;

	public CharacterMarker StartMarker { get { return _StartMarker; } }
	public CharacterMarker EndMarker { get { return _EndMarker; } }

	public bool Highlighted
	{
		get;
		set;
	}

	public bool Targetted
	{
		get;
		set;
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnDrawGizmos()
	{
		if (EditorGlobals.Instance != null && EditorGlobals.Instance.DisplayMarkers)
		{
			// Grab color from layer if available
			Color color = EditorGlobals.Instance.DoorwayColor;

			// Compute line color from that!
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

			// Draw a line to the target marker
			Gizmos.matrix = Matrix4x4.identity;
			if (_StartMarker != null)
			{
				Gizmos.DrawLine(transform.position, StartMarker.LinkPoint);
			}
			if (_EndMarker != null)
			{
				Gizmos.DrawLine(transform.position, _EndMarker.LinkPoint);
			}
		}
	}
}
