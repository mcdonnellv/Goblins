using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EckTechGames.FloatingCombatText;

[Serializable]
public class CharacterData {

	public int attributeBudget = 10;
	public static float standardCritChance = .1f;
	public static int baseLife = 10;
	public static int bodyBonusLife = 10;
	public static int baseEnergy = 5;
	public static int spiritBonusEnergy = 5;
	public string givenName;
	public string race;
	public string enemyClass;
	public int maxLife;
	public int life;
	public int maxEnergy;
	public int energy;
	public float critChance;
	public float defense;
	public CombatSigil sigil;
	public CombatUnitType unitType;

	//goblin specific
	public int body;
	public int mind;
	public int spirit;
	public int age;
	public CombatClass combatClass;

	//resistances
	public float sliceRes;
	public float crushRes;
	public float aracaneRes;
	public float darkRes;
	public float coldRes;
	public float fireRes;

	public List<CombatMove> moves = new List<CombatMove>();
	public List<BaseStatusEffect> statusEffects = new List<BaseStatusEffect>();

	public GameObject characterGameObject = null;

	public void RollStats() {
		RollAttributes();
		ApplyAttributesToStats();
		sigil = (CombatSigil)UnityEngine.Random.Range(0, 2);
	}
		
	void RollAttributes() {
		body = 0;
		mind = 0;
		spirit = 0;
		for(int i = attributeBudget; i > 0; i--) {
			switch (UnityEngine.Random.Range(0,3)) {
			case 0: body++; break;
			case 1: mind++; break;
			case 2: spirit++; break;
			}
		}
	}

	void ApplyAttributesToStats() {
		maxLife = baseLife + body * bodyBonusLife;
		maxEnergy = baseEnergy + spirit * spiritBonusEnergy;
		critChance = standardCritChance + mind * 0.02f;
		life = maxLife;
		energy = maxEnergy;
	}

	public void AssignClass(CombatClass cc) {
		combatClass = cc;
		unitType = cc.unitType;
		moves.Clear();
		foreach(Transform t in combatClass.movePrefabs){
			CombatMove cm = t.GetComponent<CombatMove>();
			moves.Add(cm);
		}

		if(characterGameObject != null) {
			Character c = characterGameObject.GetComponent<Character>();
			if(c!= null)
				c.UpdateSprite();
		}
	}
}
