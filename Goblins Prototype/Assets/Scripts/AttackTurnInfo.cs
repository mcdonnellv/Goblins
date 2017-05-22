using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class AttackTurnInfo : Object {
	public Character attacker;
	public Character defender;
	public float damage;
	public BaseStatusEffect statusEffect;
	public CombatMove move;
	public CombatMove.DamageType damageType;

	public AttackTurnInfo(Character a) { attacker = a; }
	public AttackTurnInfo(Character a, float d) { attacker = a; damage = d;}
	public AttackTurnInfo(Character a, float d, CombatMove.DamageType dt) { attacker = a; damage = d; damageType = dt;}
	public AttackTurnInfo(Character a, float d, CombatMove m) { attacker = a; damage = d; move = m;}
	public AttackTurnInfo(Character a, BaseStatusEffect s) { attacker = a; statusEffect = s;}
}
