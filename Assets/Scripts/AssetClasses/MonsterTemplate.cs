using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SAS/Monster Template")]
public class MonsterTemplate
	: ScriptableObject
{
	public Monster Prefab;
	public Prop Sword;
	public CharacterStats Stats;
}