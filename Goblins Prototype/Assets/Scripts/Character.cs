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
	public string enemyRace;
	public string enemyClass;
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
	public List<BaseStatusEffect> statusEffects = new List<BaseStatusEffect>();

	public GameObject characterGameObject = null;

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

		if(characterGameObject != null) {
			Character c = characterGameObject.GetComponent<Character>();
			if(c!= null)
				c.UpdateSprite();
		}
	}

	public void AddStatusEffect(BaseStatusEffect newStatusEffect) {
		bool found = false;
		foreach(BaseStatusEffect se in statusEffects) {
			if(se.statusEffectID == newStatusEffect.statusEffectID) {
				se.statusEffectTurnsApplied = newStatusEffect.statusEffectTurnsApplied;
				return;
			}
		}
		statusEffects.Add(newStatusEffect);
	}

	public void ProcessTurnForStatusEffects() {
		bool found = false;
		for(int i=0; i<statusEffects.Count; i++) {
			statusEffects[i].statusEffectTurnsApplied--;
			// make an onexpire event?
			if(statusEffects[i].statusEffectTurnsApplied <= 0) {
				Debug.Log("\t" + givenName + " " + statusEffects[i].statusEffectName + " has expired\n");
				statusEffects.Remove(statusEffects[i]);
			}
		}
	}
}

public class Character : MonoBehaviour {
	public CharacterData data;
	public SpriteRenderer spriteRenderer;
	public int combatPosition = 1;
	public CombatMove queuedMove = null;
	public State state = State.Unspawned;
	public Transform spawnSpot;
	public Transform headTransform;
	public Character target;
	public Shader bwShader;
	private Shader originalShader;
	public bool isPlayerCharacter;

	public enum State {
		Unspawned,
		Alive,
		Dead,
	}


	static public Transform Spawn(Transform prefab, Transform parentTransform, CharacterData cData, bool playerChar) {
		Transform spawnedChar = GameObject.Instantiate(prefab);
		spawnedChar.transform.SetParent(parentTransform);
		spawnedChar.transform.localPosition = new Vector3(0f,0f,0f);
		Character c = spawnedChar.GetComponent<Character>();
		c.spawnSpot = parentTransform;
		c.state = State.Alive;
		c.data.life = c.data.maxLife;
		c.data.energy = c.data.maxEnergy;
		c.isPlayerCharacter = playerChar;
		if(cData != null)
			c.data = cData;
		c.data.characterGameObject = spawnedChar.gameObject;
		c.UpdateSprite();
		if(c.isPlayerCharacter)
			c.data.givenName = c.data.combatClass.type.ToString();

		return spawnedChar;
	}

	public void Start() {
		originalShader = GetComponentInChildren<SpriteRenderer>().material.shader;
	}

	public void DeSpawn() {
		state = State.Unspawned;
		data.characterGameObject = null;
		Destroy(gameObject);
	}

	public void UpdateSprite() {
		if(data.combatClass.sprite != null) {
			spriteRenderer.sprite = data.combatClass.sprite;
		}
	}

	public void Idle() {
		if(state == State.Dead)
			return;
		RestoreShader();
		Animator animator = GetComponentInChildren<Animator>();
		animator.Play("Idle");
	}

	public void RestoreShader() {
		GetComponentInChildren<SpriteRenderer>().material.shader = originalShader;
	}

	public void GoBackToSpawnSpot() {
		transform.SetParent(spawnSpot, true);
		StartCoroutine(GameManager.gm.MoveOverSeconds(gameObject, spawnSpot.position, .1f));
	}


}


