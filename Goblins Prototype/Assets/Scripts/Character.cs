using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EckTechGames.FloatingCombatText;


public enum CombatClassType {
	Peon,
	Guard,
	Raider,
	Shaman,
	Ghost,
	COUNT
}



public class Character : MonoBehaviour {
	public CharacterData data;
	public SpriteRenderer spriteRenderer;
	public int combatPosition = 1;
	public CombatMove queuedMove = null;
	public State state = State.Unspawned;
	public Transform spawnSpot;
	public Transform headTransform;
	public Transform statusContainerPrefab;
	private Transform statusContainer;
	public Character target;
	public Shader bwShader;
	private Shader originalShader;
	public bool isPlayerCharacter;

	public enum State {
		Unspawned,
		Alive,
		Dead,
		Ghost,
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
		if(state == State.Dead || state == State.Ghost)
			return;
		RestoreShader();
		Animator animator = GetComponentInChildren<Animator>();
		animator.Play("Idle");
	}

	public void RestoreShader() {
		SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
		if(sr != null)
			sr.material.shader = originalShader;
	}

	public void GoBackToSpawnSpot() {
		transform.SetParent(spawnSpot, true);
		StartCoroutine(GameManager.gm.MoveOverSeconds(gameObject, spawnSpot.position, .1f));
	}

	public BaseStatusEffect AddStatusEffect(BaseStatusEffect newStatusEffect) {
		foreach(BaseStatusEffect se in data.statusEffects) {
			if(se.statusEffectID == newStatusEffect.statusEffectID) {
				se.statusEffectTurnsApplied = newStatusEffect.statusEffectTurnsApplied;
				return se;
			}
		}
		GameObject go = Instantiate(newStatusEffect.gameObject, statusContainer, false);///////////////////////
		//go.transform.Translate(new Vector3(0f,1f,0f));
		BaseStatusEffect ret = go.GetComponent<BaseStatusEffect>();
		data.statusEffects.Add(ret);
		return ret;
	}

	public void ProcessTurnForStatusEffects() {
		for(int i=0; i < data.statusEffects.Count; i++) {
			BaseStatusEffect se = data.statusEffects[i];
			if(se.statusEffectTurnsApplied == -1)
				continue;
			se.statusEffectTurnsApplied--;
			if(se.statusEffectTurnsApplied < 0) {
				BroadcastMessage("OnStatusExpired",  new AttackTurnInfo(this), SendMessageOptions.DontRequireReceiver);
				OverlayCanvasController.instance.ShowCombatText(headTransform.gameObject, CombatTextType.StatusExpired, se.statusEffectName);
				Debug.Log("\t" + data.givenName + " " + se.statusEffectName + " has expired\n");
				data.statusEffects.Remove(se);
				Destroy(se.gameObject);
				i--;
			}
		}
	}

	public void RemoveAllStatusEffects() {
		for(int i=0; i < data.statusEffects.Count; i++) {
			BaseStatusEffect se = data.statusEffects[i];
			if(se == null)
				continue;
			BroadcastMessage("OnStatusRemoved",  new AttackTurnInfo(this), SendMessageOptions.DontRequireReceiver);
			data.statusEffects.Remove(se);
			Destroy(se.gameObject);
			i--;
		}
	}

	public void RecoverFull() {
		data.life = data.maxLife;
		data.energy = data.maxEnergy;
	}

	public void Update() {
		if(statusContainer == null)
			statusContainer = GameObject.Instantiate(statusContainerPrefab, GameManager.gm.arena.combatUI.statusContainers, false);
		statusContainer.position = Camera.main.WorldToScreenPoint(headTransform.position + new Vector3(0f,.7f,0f));
	}

}


