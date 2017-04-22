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

	public virtual float OnDamageDealtToMeCalc(AttackTurnInfo ati) {return ati.damage;}
	public virtual float OnDamageDealtByMeCalc(AttackTurnInfo ati) {return ati.damage;}
	public virtual float OnDamageTakenCalc(AttackTurnInfo ati) {return ati.damage;}
	public virtual void OnIGotTargetted(AttackTurnInfo ati) {}
	public virtual void OnStatusEffectAddedToMe(AttackTurnInfo ati) {}
	public virtual void OnMyTurnStarted(AttackTurnInfo ati) {}

}

