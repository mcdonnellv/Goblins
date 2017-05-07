using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class DotStatusEffect : BaseStatusEffect {
	CombatMove move;
	Character applier;

	public override void OnStatusEffectAddedToMe(AttackTurnInfo ati) {
		if(ati.statusEffect.statusEffectID == statusEffectID) {
			move = ati.attacker.queuedMove;
			applier = ati.attacker;
		}
	}

	public override void OnTurnEnded(AttackTurnInfo ati) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		Character recipient = ati.attacker;
		CombatMath cm = GameManager.gm.arena.cm;
		float damage = cm.RollForDamage(move, applier, recipient);
		int finalDamage = Mathf.FloorToInt(damage);
		cm.ApplyDamage(finalDamage, recipient.data);
		occ.ShowCombatText(recipient.headTransform.gameObject, CombatTextType.Hit, finalDamage.ToString());
		recipient.RefreshLifeBar();
	}
}
