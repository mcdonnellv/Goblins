using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMath : MonoBehaviour {

	public float baseCritDamageMultiplier = 1.5f;
	public float baseCritDamageMultiplierIncrement = .5f;
	public float baseCritChance = 0.1f;
	public float standardUnitTypeAdvantage = .2f;
	public float standardSigilAdvantage = .1f;

	public bool RollForHit(CharacterData attacker, CharacterData defender, CombatMove move) {
		//magical ranged attacks always hit
		if(move.rangeType == CombatMove.RangeType.Ranged && move.damageType == CombatMove.DamageType.Magical)
			return true;
		
		float roll = UnityEngine.Random.Range(0f,1f);
		if (roll > defender.defense)
			return true;
		return false;
	}

	public bool RollForCrit(CombatMove combatMove, CharacterData attacker) {
		if(combatMove.canCrit == false)
			return false;
		float roll = UnityEngine.Random.Range(0f,1f);
		float mindModified = Mathf.Max(0f, attacker.mind - 4f);
		float chance = combatMove.critChance + (.01f * Mathf.Pow(mindModified, 1.5f));
		if (roll < chance)
			return true;
		return false;
	}

	public float RollForDamage(int moveDamage, Character attacker, Character defender, CombatMove.DamageType damageType, float critVal) {
		float typeAdvantage = 0f;
		float sigilAdvantage = 0f;

		if(attacker != null) {
			typeAdvantage = Advantage(attacker.data.unitType, defender.data.unitType);
			sigilAdvantage = Advantage(attacker.data.sigil, defender.data.sigil);
		}

		float damage = moveDamage * (1f + typeAdvantage + sigilAdvantage);
		damage = Mathf.Max(0f, damage);

		// status effects may alter the attack's damage value
		AttackTurnInfo ati = new AttackTurnInfo(attacker, damage, damageType);
		defender.statusContainer.BroadcastMessage("OnDamageDealtToMeCalc", ati, SendMessageOptions.DontRequireReceiver);

		if(attacker != null)
			attacker.statusContainer.BroadcastMessage("OnDamageDealtByMeCalc", ati, SendMessageOptions.DontRequireReceiver);

		//factor in crit
		ati.damage *= critVal;

		return ati.damage ;
	}


	public void ApplyDamage(int damage, CharacterData defender) {
		defender.life = Mathf.Max(0, defender.life - damage);
	}

	public int ApplyHeal(int heal, CharacterData recipient) {
		int damage = recipient.maxLife - recipient.life;
		recipient.life = Mathf.Min(recipient.maxLife, recipient.life + heal);
		return Mathf.Min(heal, damage);
	}

	public float Advantage(CombatUnitType a, CombatUnitType b) {
		if(a == CombatUnitType.Armored && b == CombatUnitType.Assault)
			return standardUnitTypeAdvantage;
		if(a == CombatUnitType.Assault && b == CombatUnitType.MagicUser)
			return standardUnitTypeAdvantage;
		if(a == CombatUnitType.MagicUser && b == CombatUnitType.Armored)
			return standardUnitTypeAdvantage;
		
		if(a == CombatUnitType.Armored && b == CombatUnitType.MagicUser)
			return -standardUnitTypeAdvantage;
		if(a == CombatUnitType.Assault && b == CombatUnitType.Armored)
			return -standardUnitTypeAdvantage;
		if(a == CombatUnitType.MagicUser && b == CombatUnitType.Assault)
			return -standardUnitTypeAdvantage;
		return 0f;
	}

	public float Advantage(CombatSigil a, CombatSigil b) {
		if(a == CombatSigil.Sun && b == CombatSigil.Moon)
			return standardSigilAdvantage;
		if(a == CombatSigil.Moon && b == CombatSigil.Star)
			return standardSigilAdvantage;
		if(a == CombatSigil.Star && b == CombatSigil.Sun)
			return standardSigilAdvantage;

		if(a == CombatSigil.Sun && b == CombatSigil.Star)
			return -standardSigilAdvantage;
		if(a == CombatSigil.Moon && b == CombatSigil.Sun)
			return -standardSigilAdvantage;
		if(a == CombatSigil.Star && b == CombatSigil.Moon)
			return -standardSigilAdvantage;
		
		return 0f;
	}
}