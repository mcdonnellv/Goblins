using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class CombatMove : MonoBehaviour {
	public enum MoveCategory {
		Attack,
		Defense,
		Special,
	}

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

	public enum TargetType {
		Opponent,
		Self,
		RandomAlly,
		RandomOpponent,
		MostDamagedAlly,
		AllyBehind,
		None,
	}

	public enum RangeType {
		Melee,
		Ranged,
	}

	public string moveName;
	public string description;
	public int effectiveness = 1;
	public float critChance = .1f;
	public int weight = 10;
	public int energyCost = 2;
	public Sprite sprite;
	public MoveCategory moveCategory = MoveCategory.Attack;
	public MoveType moveType = MoveType.Damage;
	public RangeType rangeType = RangeType.Melee;
	public TargetType targetType = TargetType.Opponent;
	public DamageType damageType = DamageType.Slice;
	public List<BaseStatusEffect> moveStatusEffects = new List<BaseStatusEffect>(); //this move may apply 1 to many status effects on its target
	public bool displaceOpponent;
	public bool isDot = false;
	public float workingDamage = 0f;
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

	public Color ColorFromDamageType () {
		switch(damageType) {
		case DamageType.Slice: return Color.magenta;
		case DamageType.Crush: return Color.yellow;
		case DamageType.Arcane: return Color.cyan;
		case DamageType.Dark: return new Color(1f,0f,1f);
		case DamageType.Fire: return Color.red;
		case DamageType.Cold: return  Color.blue;
		}
		return Color.white;
	}

	public static Color ColorFromMovePosition (int pos) {
		switch(pos) {
		case 1: return new Color(0.804f, 0.918f, 0.804f);
		case 2: return new Color(0.918f, 0.678f, 0.376f);
		case 3: return new Color(0.902f, 0.804f, 0.918f);
		}
		return Color.white;
	}

	static public Sprite SpriteForMoveCategory(MoveCategory c) {
		Sprite s = null;
		switch(c) {
		case MoveCategory.Attack: s = Resources.Load<Sprite>("Icons/UI_Versus"); break;
		case MoveCategory.Defense: s = Resources.Load<Sprite>("Icons/Equipment_Shield"); break;
		case MoveCategory.Special: s = Resources.Load<Sprite>("Icons/Rewards_Diamond"); break;
		}
		return s;
	}

	public static Color ColorForMoveCategory (MoveCategory c) {
		switch(c) {
		case MoveCategory.Attack: return new Color(0.627f, 0.239f, 0.239f);
		case MoveCategory.Defense: return new Color(0.239f, 0.627f, 0.349f);
		case MoveCategory.Special: return new Color(0.624f, 0.239f, 0.627f);
		}
		return Color.white;
	}
}
