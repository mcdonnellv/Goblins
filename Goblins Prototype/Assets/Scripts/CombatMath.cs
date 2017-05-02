using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatMath : MonoBehaviour {

	public float baseCritDamageMultiplier = 1.5f;
	public float baseCritDamageMultiplierIncrement = .5f;
	public float baseCritChance = 0.1f;

	public bool RollForHit(CharacterData attacker, CharacterData defender) {
		float roll = UnityEngine.Random.Range(0f,1f);
		if (roll > defender.defense)
			return true;
		return false;
	}

	public bool RollForCrit(CombatMove combatMove, CharacterData attacker) {
		float roll = UnityEngine.Random.Range(0f,1f);
		float mindModified = Mathf.Max(0f, attacker.mind - 4f);
		float chance = combatMove.critChance + (.01f * Mathf.Pow(mindModified, 1.5f));
		if (roll < chance)
			return true;
		return false;
	}

	public float RollForDamage(CombatMove combatMove, Character attacker, Character defender) {
		if (combatMove == null)
			return 0;
		float resist = GetResistForDamageType(combatMove.damageType, defender.data);
		float damage = combatMove.effectiveness * (1f - resist);
		damage = Mathf.Max(0f, damage);
		// status effects may alter the attack's damage value
		AttackTurnInfo ati = new AttackTurnInfo(attacker, damage);
		defender.statusContainer.BroadcastMessage("OnDamageDealtToMeCalc", ati, SendMessageOptions.DontRequireReceiver);
		attacker.statusContainer.BroadcastMessage("OnDamageDealtByMeCalc", ati, SendMessageOptions.DontRequireReceiver);
		return ati.damage;
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

	public void ApplyDamage(int damage, CharacterData defender) {
		defender.life = Mathf.Max(0, defender.life - damage);
	}

	public int ApplyHeal(int heal, CharacterData recipient) {
		int damage = recipient.maxLife - recipient.life;
		recipient.life = Mathf.Min(recipient.maxLife, recipient.life + heal);
		return Mathf.Min(heal, damage);
	}
}