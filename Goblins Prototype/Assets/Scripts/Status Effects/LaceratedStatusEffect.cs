using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaceratedStatusEffect : BaseStatusEffect {
	
	public override float OnDamageDealtToMeCalc(AttackTurnInfo ati) {
		if(ati.attacker.queuedMove.moveName == "Wound") 
			ati.damage = ati.damage * statusEffectPower;
		return ati.damage;
	}
}