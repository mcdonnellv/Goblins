using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CursedStatusEffect : BaseStatusEffect {
	public override float OnDamageDealtToMeCalc(AttackTurnInfo ati) {
		ati.damage = ati.damage * statusEffectPower;
		return ati.damage;
	}
}

