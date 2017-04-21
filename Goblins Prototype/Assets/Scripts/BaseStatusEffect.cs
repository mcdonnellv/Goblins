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

	public virtual float OnDamageCalc(CombatMove move, float damage) {return damage;}

}

