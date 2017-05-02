using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BracingStatusEffect : BaseStatusEffect {

	public BashBonusStatusEffect bashBonusStatusEffectPrefab;

	public override float OnDamageTakenCalc(AttackTurnInfo ati) {
		if(ati.attacker.queuedMove.damageType == CombatMove.DamageType.Crush || ati.attacker.queuedMove.damageType == CombatMove.DamageType.Slice) {
			ati.damage *= statusEffectPower;
			BashBonusStatusEffect bbse = null;
			foreach(BaseStatusEffect se in owner.data.statusEffects) {
				if(se.statusEffectID == bashBonusStatusEffectPrefab.statusEffectID) {
					bbse = (BashBonusStatusEffect)se;
					break;
				}
			}

			if(bbse == null)
				bbse = (BashBonusStatusEffect)owner.AddStatusEffect(bashBonusStatusEffectPrefab);
			bbse.damageStored += ati.damage;
		}
		return ati.damage;
	}
}



