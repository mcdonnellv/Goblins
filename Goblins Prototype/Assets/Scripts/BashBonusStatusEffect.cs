using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BashBonusStatusEffect : BaseStatusEffect {
	public float damageStored = 0f;
	public CombatMove bashMovePrefab;

	public override float OnDamageDealtByMeCalc(AttackTurnInfo ati) {
		if(ati.attacker.queuedMove.moveName == bashMovePrefab.moveName) {
			ati.damage = ati.damage + damageStored;
			statusEffectTurnsApplied = 0;
		}
		return ati.damage;
	}
}
