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
		Cold,
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

	public string GenerateDesciption () {
		switch(moveType){
		case MoveType.Damage : 
			description = "Deal " + effectiveness.ToString();
			switch(damageType) {
			case DamageType.Slice : description += " slicing damage"; break;
			case DamageType.Crush : description += " crushing damage"; break;
			case DamageType.Arcane : description += " arcane damage"; break;
			case DamageType.Dark : description += " dark damage"; break;
			case DamageType.Cold : description += " cold damage"; break;
			case DamageType.Fire : description += " fire damage"; break;
			}
			break;
		case MoveType.Heal : 
			description = "Heal " + effectiveness.ToString() + " damage";
			break;
		}

		return description;
	}
}
