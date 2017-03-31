using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum CombatClassType {
	Peon,
	Guard,
	Raider,
	Shaman,
	COUNT
}


[Serializable]
public class CharacterData {

	public int attributeBudget = 10;
	public static float standardCritChance = .1f;

	public string givenName;
	public int maxLife;
	public int life;
	public int maxEnergy;
	public int energy;
	public float critChance;
	public float defense;

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

	public void RollStats() {
		RollAttributes();
		ApplyAttributesToStats();
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
		maxLife = 100 + body * 15;
		maxEnergy = 50 + spirit * 10;
		critChance = standardCritChance + mind * 0.02f;
		life = maxLife;
		energy = maxEnergy;
	}

	public void AssignClass(CombatClass cc) {
		combatClass = cc;
		moves.Clear();
		foreach(Transform t in combatClass.movePrefabs){
			CombatMove cm = t.GetComponent<CombatMove>();
			moves.Add(cm);
		}

	}
}

public class Character : MonoBehaviour {
	public CharacterData data;
}
