using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CombatFormulas
{
	public static float ComputeDamage(CharacterStats attackerStats, CharacterStats targetStats)
	{
		// Use some trivial math for now!
		return Mathf.Max(0.0f, attackerStats.AP - targetStats.Armor);
	}
}
