using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// An asset class that wraps character stats so we can create templates!
/// </summary>
[CreateAssetMenu(menuName = "SAS/Hero Template")]
public class HeroTemplate
	: ScriptableObject
{
	public Hero Prefab;
	public Prop Sword;
	public CharacterStats Stats;
}

