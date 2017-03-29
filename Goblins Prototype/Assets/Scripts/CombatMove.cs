using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatMove : MonoBehaviour {
	public enum MoveType {
		Damage,
		Heal,
		Idle,
	}

	public enum DamageType {
		Slice,
		Crush,
		Arcane,
		Dark,
		Ice,
		Fire,
		None,
	}

	public string moveName;
	public string description;
	public int effectiveness = 1;
	public float critChance = .1f;
	public int weight = 10;
	public int energyCost = 2;
	public MoveType moveType = MoveType.Damage;
	public DamageType damageType = DamageType.Slice;
}
