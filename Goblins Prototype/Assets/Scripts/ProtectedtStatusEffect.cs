using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProtectedtStatusEffect : BaseStatusEffect {

	public Character protector;

	public override void OnAdd(Character caster) {
		protector = caster;
	}

	public override void OnTargetted(CombatMove move, Character enemy) {
		if(move.targetType == CombatMove.TargetType.Opponent && protector != null && protector.state != Character.State.Dead)
			enemy.target = protector;
	}

}
