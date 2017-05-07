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

	public AttackTurnInfo(Character a) { attacker = a; }
	public AttackTurnInfo(Character a, float d) { attacker = a; damage = d;}
	public AttackTurnInfo(Character a, BaseStatusEffect s) { attacker = a; statusEffect = s;}
}
