using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LaceratedStatusEffect : BaseStatusEffect {
	
	public override float OnDamageCalc(CombatMove move, float damage) {
		if(move.moveName == "Wound") 
			damage = damage * statusEffectPower;
		return damage;
	}
}
