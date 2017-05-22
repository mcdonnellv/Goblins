using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BashBonusStatusEffect : BaseStatusEffect {
	public float damageStored = 0f;
	public CombatMove bashMovePrefab;

	public override void OnDamageDealtByMeCalc(AttackTurnInfo ati) {
		if(ati.attacker.queuedMove.moveName == bashMovePrefab.moveName) {
			ati.damage = ati.damage + damageStored;
			statusEffectTurnsApplied = 0;
		}
	}

	public override string GetDescription() { 
		return string.Format(statusEffectDescription, Mathf.RoundToInt(damageStored)); 
	}
}
