using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatMove : MonoBehaviour {
	public enum MoveType {
		Damage,
		Heal,
	}

	public enum DamageType {
		Slice,
		Crush,
		Arcane,
		Dark,
		Ice,
		Fire,
	}

	public string name;
	public string description;
	public int effectiveness;
	public MoveType moveType = MoveType.Damage;
	public DamageType damageType = DamageType.Slice;
}
