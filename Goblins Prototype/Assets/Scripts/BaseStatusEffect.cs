using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseStatusEffect : MonoBehaviour {
	public string statusEffectName;
	public string statusEffectDescription;
	public int statusEffectID;
	public float statusEffectPower;
	public int statusEffectTurnsApplied;

	public virtual float OnDamageDealtToMeCalc(CombatMove move, float damage) {return damage;}
	public virtual float OnDamageDealtByMeCalc(CombatMove move, float damage) {return damage;}
	public virtual float OnDamageTakenCalc(CombatMove move, float damage) {return damage;}
	public virtual void OnTargetted(CombatMove move, Character enemy) {}
	public virtual void OnAdd(Character caster) {}

}

