using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProtectedStatusEffect : BaseStatusEffect {

	Character protector;

	public override void OnStatusEffectAddedToMe(AttackTurnInfo ati) {
		if(ati.statusEffect.statusEffectID == statusEffectID)
			protector = ati.attacker;
	}

	public override void OnIGotTargetted(AttackTurnInfo ati) {
		CombatMove move = ati.attacker.queuedMove;

		//only care if we are targeted by our enemies
		if(move.targetType != CombatMove.TargetType.Opponent)
			return;
			
		if(protector == null || protector.state == Character.State.Dead)
			return;

		//change the enemies target to a protector
		ati.attacker.target = protector;
	}

}

