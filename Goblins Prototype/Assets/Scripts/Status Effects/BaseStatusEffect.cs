using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BaseStatusEffect : MonoBehaviour {
	public Character owner;
	public string statusEffectName;
	public string statusEffectDescription;
	public int statusEffectID;
	public float statusEffectPower;
	public int statusEffectTurnsApplied;
	public CombatMove.DamageType statusEffectDamageType;

	public virtual void OnDamageDealtToMeCalc(AttackTurnInfo ati) {}
	public virtual void OnDamageDealtByMeCalc(AttackTurnInfo ati) {}
	public virtual void OnIGotTargetted(AttackTurnInfo ati) {}
	public virtual void OnStatusEffectAddedToMe(AttackTurnInfo ati) {}
	public virtual void OnMyTurnStarted(AttackTurnInfo ati) {}
	public virtual void OnStatusExpired(AttackTurnInfo ati) {}
	public virtual void OnStatusRemoved(AttackTurnInfo ati) {}
	public virtual void OnTurnEnded(AttackTurnInfo ati) {}
	public virtual string GetDescription() { return statusEffectDescription; }

	public void Tapped() {
		GameManager.gm.arena.combatUI.ShowToolTip(statusEffectName, GetDescription(), statusEffectTurnsApplied + 1, 3f);
	}

}