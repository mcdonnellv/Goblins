using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveRollBonusStatusEffect : BaseStatusEffect {
	public override void OnDamageDealtByMeCalc(AttackTurnInfo ati) {
		ati.damage *= statusEffectPower;
	}
}
