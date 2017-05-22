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
		Physical,
		Magical
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
	public DamageType damageType = DamageType.Physical;
	public List<BaseStatusEffect> moveStatusEffects = new List<BaseStatusEffect>(); //this move may apply 1 to many status effects on its target
	public bool displaceOpponent;
	public bool isDot = false;
	public float workingDamage = 0f;
	public bool canCrit = true;


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
