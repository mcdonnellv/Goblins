using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class BracingStatusEffect : BaseStatusEffect {
	CombatMove move;
	public BashBonusStatusEffect bashBonusStatusEffectPrefab;

	public override void OnStatusEffectAddedToMe(AttackTurnInfo ati) {
		if(ati.statusEffect.statusEffectID == statusEffectID) {
			move = ati.attacker.queuedMove;
		}

		statusEffectPower += move.effectiveness * .01f;
		statusEffectPower = Mathf.Min(statusEffectPower, .8f);
	}
		
	public override void OnDamageDealtToMeCalc(AttackTurnInfo ati) {
		if(ati.damageType != CombatMove.DamageType.Physical)
			return;
		
		float originalDamage = ati.damage;
		//lessen damage
		ati.damage *= (1f - statusEffectPower);

		BashBonusStatusEffect bbse = AddBashBonusStatus();
		float mitigatedDamage = originalDamage - ati.damage;
		if(mitigatedDamage > 0) {
			StartCoroutine(ShowMitigatedDamage(Mathf.RoundToInt(mitigatedDamage), 1f));
			bbse.damageStored += mitigatedDamage;
		}
	}

	BashBonusStatusEffect AddBashBonusStatus() {
		foreach(BaseStatusEffect se in owner.data.statusEffects) {
			if(se.statusEffectID == bashBonusStatusEffectPrefab.statusEffectID) {
				return (BashBonusStatusEffect)se;
			}
		}
		return (BashBonusStatusEffect)owner.AddStatusEffect(bashBonusStatusEffectPrefab);
	}

	IEnumerator ShowMitigatedDamage(int mitigatedDamage, float timer) {
		yield return new WaitForSeconds(timer);
		OverlayCanvasController.instance.ShowCombatText(owner.headTransform.gameObject, 
			CombatTextType.StatusAppliedGood, 
			"(" + mitigatedDamage + " absorbed)");
	}

	public override string GetDescription() { 
		return string.Format(statusEffectDescription, Mathf.RoundToInt(statusEffectPower * 100f)); 
	}
}



