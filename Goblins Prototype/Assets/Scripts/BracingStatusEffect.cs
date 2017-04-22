using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BracingStatusEffect : BaseStatusEffect {

	public BashBonusStatusEffect bashBonusStatusEffectPrefab;

	public override float OnDamageTakenCalc(CombatMove move, float damage) {
		if(move.damageType == CombatMove.DamageType.Crush || move.damageType == CombatMove.DamageType.Slice) {
			damage *= statusEffectPower;
			Character character = gameObject.GetComponentInParent<Character>();
			BashBonusStatusEffect bbse = null;
			foreach(BaseStatusEffect se in character.data.statusEffects) {
				if(se.statusEffectID == bashBonusStatusEffectPrefab.statusEffectID) {
					bbse = (BashBonusStatusEffect)se;
					break;
				}
			}

			if(bbse == null)
				bbse = (BashBonusStatusEffect)character.AddStatusEffect(bashBonusStatusEffectPrefab);
			bbse.damageStored += damage;
		}
		return damage;
	}
}



