using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BashBonusStatusEffect : BaseStatusEffect {
	public float damageStored = 0f;
	public CombatMove bashMovePrefab;

	public override float OnDamageDealtByMeCalc(CombatMove move, float damage) {
		if(move.moveName == bashMovePrefab.moveName) {
			damage = damage + damageStored;
			statusEffectTurnsApplied = 0;
		}
		return damage;
	}
}
