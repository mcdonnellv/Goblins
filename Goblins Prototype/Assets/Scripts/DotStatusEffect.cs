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

	public override void OnMyTurnStarted(AttackTurnInfo ati) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		CombatMath cm = GameManager.gm.arena.cm;
		float damage = cm.RollForDamage(move, applier, ati.attacker);
		int finalDamage = Mathf.FloorToInt(damage);
		cm.ApplyDamage(finalDamage, ati.attacker.data);
		ati.attacker.RefreshLifeBar();
		occ.ShowCombatText(ati.attacker.headTransform.gameObject, CombatTextType.Hit, finalDamage.ToString());
	}
}
