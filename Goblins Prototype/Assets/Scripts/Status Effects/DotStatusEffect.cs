using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class DotStatusEffect : BaseStatusEffect {
	Character applier;

	public override void OnStatusEffectAddedToMe(AttackTurnInfo ati) {
		if(ati.statusEffect.statusEffectID == statusEffectID) {
			statusEffectPower = (float)ati.attacker.queuedMove.effectiveness;
			statusEffectDamageType = ati.attacker.queuedMove.damageType;
			applier = ati.attacker;
		}
	}

	public override void OnTurnEnded(AttackTurnInfo ati) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		Character recipient = ati.attacker;
		CombatMath cm = GameManager.gm.arena.cm;
		float workingDamage = cm.RollForDamage((int)statusEffectPower, applier, recipient, statusEffectDamageType, 1f);

		AttackTurnInfo newAti = new AttackTurnInfo(applier, workingDamage, statusEffectDamageType);

		int finalDamage = Mathf.RoundToInt(newAti.damage);
		cm.ApplyDamage(finalDamage, recipient.data);
		occ.ShowCombatText(recipient.headTransform.gameObject, CombatTextType.StatusAppliedBad, statusEffectName + "\n" + finalDamage.ToString());
		recipient.RefreshLifeBar();
	}
	

	public override string GetDescription() { 
		return string.Format(statusEffectDescription, statusEffectPower); 
	}
}
