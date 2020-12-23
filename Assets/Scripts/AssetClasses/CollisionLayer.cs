using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple asset class to store Collision layer information
/// This is used during navmesh generation and for debug rendering
/// </summary>
[CreateAssetMenu(menuName = "SAS/Collision Layer")]
public class CollisionLayer
	: ScriptableObject
{
	public PhysicMaterial PhysicsMaterial;
	public Color DisplayColor;
	public bool Walkable = true;
}
