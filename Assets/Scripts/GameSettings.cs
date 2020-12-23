using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(menuName = "SAS/Game Settings")]
public class GameSettings : ScriptableObject
{
	[Header("Game")]
	public HeroTemplate HeroTemplate;

	[Header("Audio")]
	public AudioClip ValidCommand;
	public AudioClip InvalidCommand;
	public AudioMixerSnapshot MainSnapshot;
	public AudioMixerSnapshot SilenceSnapshot;

	[Header("Camera")]
	// The camera position (in world space) relative to the character
	public Vector3 CameraOffset = new Vector3(5.4f, 5.2f, -2.34f);
	public float SmoothDamp = 0.2f;

	[Header("Combat")]
	public float AngleBetweenPositions = 45.0f;
	public float MonsterDistance = 1.5f;

	public float AngleBetweenHoldingPositions = 22.5f;
	public float MonsterHoldingDistance = 3.5f;

	public int MaxConcurrentSpawns = 5;

	[Header("Cursor")]
	public Texture2D DefaultCursor;
	public Vector2 DefaultCursorHotspot;

	public Texture2D MoveCursor;
	public Vector2 MoveCursorHotspot;

	public Texture2D AttackCursor;
	public Vector2 AttackCursorHotspot;

	public Texture2D DoorwayCursor;
	public Vector2 DoorwayCursorHotspot;

	[Header("FX")]
	public GameObject ClickAknowledgePrefab;
	public GameObject SwordImpactPrefab;

	[Header("Input")]
	[Header("Object")]
	public int MaxDeadMonsterCount = 30;

	[Header("UI")]
	public MonsterUI MonsterUIPrefab;
	public HeroUI HeroUIPrefab;
	public Material[] HighlightMaterials;
	public Material[] TargettedMaterials;

	[Header("Mouse")]
	public string WalkableLayerName = "Walkable";
	public string NonWalkableLayerName = "NonWalkable";
	public string MonsterLayerName = "Monster";
	public string InteractableLayerName = "Interactable";
	public float NavMeshDistance = 0.2f;

	[Header("Collision Layers")]
	public CollisionLayer DefaultWalkableLayer;
	public CollisionLayer DefaultNonWalkableLayer;
}

