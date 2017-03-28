using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterData {
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

	//resistances
	public float sliceRes;
	public float crushRes;
	public float aracaneRes;
	public float darkRes;
	public float coldRes;
	public float fireRes;

	public List<CombatMove> moves = new List<CombatMove>();
}

public class Character : MonoBehaviour {
	public CharacterData data;
}
