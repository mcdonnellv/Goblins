using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaceratedStatusEffect : BaseStatusEffect {
	
	public override float OnDamageDealtToMeCalc(AttackTurnInfo ati) {
		string moveName = ati.attacker.queuedMove.moveName;
		if(moveName == "Wound" || moveName== "Open Wound") 
			ati.damage = ati.damage * statusEffectPower;
		return ati.damage;
	}
}