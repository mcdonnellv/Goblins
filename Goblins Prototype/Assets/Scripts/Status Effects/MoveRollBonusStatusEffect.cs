using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveRollBonusStatusEffect : BaseStatusEffect {
	public override float OnDamageDealtByMeCalc(AttackTurnInfo ati) {
		return ati.damage * this.statusEffectPower;
	}
}
