using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class BracingStatusEffect : BaseStatusEffect {

	public BashBonusStatusEffect bashBonusStatusEffectPrefab;

	public override void OnDamageTakenCalc(AttackTurnInfo ati) {
		if(ati.attacker.queuedMove.damageType == CombatMove.DamageType.Crush || ati.attacker.queuedMove.damageType == CombatMove.DamageType.Slice) {
			CombatMove move = ati.attacker.queuedMove;
			float originalDamage = move.workingDamage;
			move.workingDamage *= statusEffectPower;
			BashBonusStatusEffect bbse = null;
			foreach(BaseStatusEffect se in owner.data.statusEffects) {
				if(se.statusEffectID == bashBonusStatusEffectPrefab.statusEffectID) {
					bbse = (BashBonusStatusEffect)se;
					break;
				}
			}

			if(bbse == null)
				bbse = (BashBonusStatusEffect)owner.AddStatusEffect(bashBonusStatusEffectPrefab);
			float mitigatedDamage = originalDamage - move.workingDamage;
			if(mitigatedDamage > 0)
				StartCoroutine(ShowMitigatedDamage(Mathf.RoundToInt(mitigatedDamage), 1f));
			bbse.damageStored += mitigatedDamage;
		}
	}

	IEnumerator ShowMitigatedDamage(int mitigatedDamage, float timer) {
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		OverlayCanvasController.instance.ShowCombatText(owner.headTransform.gameObject, 
			CombatTextType.StatusAppliedGood, 
			"(" + mitigatedDamage + " absorbed)");
	}
}



