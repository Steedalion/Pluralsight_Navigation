using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Editor Globals stores things like colors for rendering, etc...
/// </summary>
public class EditorGlobals
	: SingletonBehaviour<EditorGlobals>
{
	[Header("Behavior Control")]
	public bool DisplayMarkers = true;

	[Header("Gizmo Rendering")]
	public Color WalkableCollisionBoxColor = Color.blue;
	public Color NonWalkableCollisionBoxColor = Color.yellow;
	public Color CleanupNavmeshCollisionBoxColor = Color.red;
	public Color CharacterMarkerDisplayColor = Color.yellow;
	public Color MonsterSpawnDisplayColor = Color.cyan;
	public Color DoorwayColor = Color.yellow;
	[Range(0.0f, 1.0f)]
	public float BoxWireLineAlpha_Selected = 1.0f;
	[Range(0.0f, 1.0f)]
	public float BoxWireLineAlpha = 0.5f;
	[Range(0.0f, 1.0f)]
	public float CharWireLineAlpha_Selected = 0.1f;
	[Range(0.0f, 1.0f)]
	public float CharWireLineAlpha = 0.03f;
}
