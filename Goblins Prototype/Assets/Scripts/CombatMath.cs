using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMath : MonoBehaviour {

	public float baseCritDamage = 1.5f;
	public float baseCritChance = 0.1f;

	public bool RollForHit(CharacterData attacker, CharacterData defender) {
		float roll = UnityEngine.Random.Range(0f,1f);
		if (roll > defender.defense)
			return true;
		return false;
	}

	public bool RollForCrit(CharacterData attacker, CharacterData defender) {
		float roll = UnityEngine.Random.Range(0f,1f);
		float mindModified = Mathf.Max(0f, attacker.mind - 4f);
		float chance = baseCritChance + (.01f * Mathf.Pow(mindModified, 1.5f));
		if (roll < chance)
			return true;
		return false;
	}

	public float RollForDamage(CombatMove combatMove, CharacterData attacker, CharacterData defender) {
		if (combatMove == null)
			return 0;
		float resist = GetResistForDamageType(combatMove.damageType, defender);
		float damage = combatMove.effectiveness * (1f + resist);
		damage = Mathf.Max(0f, damage);
		// status effects may alter the attack's damage value
		foreach(BaseStatusEffect se in defender.statusEffects)
			damage = se.OnDamageDealtToMeCalc(combatMove, damage);

		foreach(BaseStatusEffect se in attacker.statusEffects)
			damage = se.OnDamageDealtByMeCalc(combatMove, damage);
		
		return damage;
	}

	public float GetResistForDamageType(CombatMove.DamageType dt, CharacterData defender) {
		switch(dt) {
		case CombatMove.DamageType.Slice: return defender.sliceRes;
		case CombatMove.DamageType.Crush: return defender.crushRes;
		case CombatMove.DamageType.Arcane: return defender.aracaneRes;
		case CombatMove.DamageType.Dark: return defender.darkRes;
		case CombatMove.DamageType.Cold: return defender.coldRes;
		case CombatMove.DamageType.Fire: return defender.fireRes;
		}
		return 0f;
	}

	public void ApplyDamage(CombatMove combatMove, int damage, CharacterData defender) {
		defender.life = Mathf.Max(0, defender.life - damage);
	}
}