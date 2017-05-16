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

public enum CombatUnitType {
	Armored,
	Assault,
	MagicUser,
	NoType,
}

public enum CombatSigil {
	Sun,
	Moon,
	Star,
	NoSigil
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
	public Transform statusContainer;
	public Transform lifeBarPrefab;
	public Transform lifeBar;
	public Transform targetPointer;
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

	static public Color ColorForUnitType(CombatUnitType ut) {
		switch(ut) {
		case CombatUnitType.Armored: return new Color(0.714f, 0.267f, 0.337f);
		case CombatUnitType.Assault: return new Color(0.165f, 0.584f, 0.325f);
		case CombatUnitType.MagicUser: return new Color(0.282f, 0.451f, 0.82f);
		}
		return Color.gray;
	}

	static public Sprite SpriteForUnitType(CombatUnitType ut) {
		Sprite s = null;
		switch(ut) {
		case CombatUnitType.Armored: s = Resources.Load<Sprite>("Icons/Equipment_Helmet"); break;
		case CombatUnitType.Assault: s = Resources.Load<Sprite>("Icons/Weapons_Sword"); break;
		case CombatUnitType.MagicUser: s = Resources.Load<Sprite>("Icons/Weapons_Staff"); break;
		}
		return s;
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
		if(c.data.sigil == CombatSigil.NoSigil)
			c.data.sigil = (CombatSigil)UnityEngine.Random.Range(0, 2);
		c.data.characterGameObject = spawnedChar.gameObject;
		c.UpdateSprite();

		if(c.isPlayerCharacter)
			c.data.givenName = c.data.race + c.data.combatClass.type.ToString();	
		c.name = c.data.givenName;

		float dm = GameManager.gm.enemies.difficutlyModifier;
		if(!playerChar && dm > 1f) {
			c.data.maxLife = Mathf.FloorToInt(c.data.maxLife * dm);
			c.data.life = c.data.maxLife;
		}
		return spawnedChar;
	}

	public void Start() {
		originalShader = GetComponentInChildren<SpriteRenderer>().material.shader;
	}

	public void DeSpawn() {
		state = State.Unspawned;
		Cleanup();
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
		GameObject go = Instantiate(newStatusEffect.gameObject, statusContainer, false);
		BaseStatusEffect ret = go.GetComponent<BaseStatusEffect>();
		ret.owner = this;
		data.statusEffects.Add(ret);
		return ret;
	}

	public void RefreshLifeBar() {
		Vector2 s1 = lifeBar.GetComponent<RectTransform>().sizeDelta;
		float v = s1.x;
		float curval = data.life;
		float totVal = data.maxLife;
		Vector2 s = lifeBar.GetChild(1).GetComponent<RectTransform>().sizeDelta;
		lifeBar.GetChild(1).GetComponent<RectTransform>().sizeDelta = new Vector2(v * curval/totVal, s.y);

		if(isPlayerCharacter)
			GameManager.gm.arena.combatUI.GetPanelForPlayer(this).RefreshBars();

		if(target != null && target.isPlayerCharacter) {
			GoblinCombatPanel gcp = GameManager.gm.arena.combatUI.GetPanelForPlayer(target);
			if(gcp != null)
				gcp.RefreshBars();
		}
	}

	public void ProcessTurnForStatusEffects() {
		for(int i=0; i < data.statusEffects.Count; i++) {
			BaseStatusEffect se = data.statusEffects[i];
			if(se.statusEffectTurnsApplied == -1)
				continue;
			se.statusEffectTurnsApplied--;
			if(se.statusEffectTurnsApplied < 0) {
				statusContainer.BroadcastMessage("OnStatusExpired",  new AttackTurnInfo(this), SendMessageOptions.DontRequireReceiver);
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
			statusContainer.BroadcastMessage("OnStatusRemoved",  new AttackTurnInfo(this), SendMessageOptions.DontRequireReceiver);
			data.statusEffects.Remove(se);
			Destroy(se.gameObject);
			i--;
		}
	}

	public void RecoverFull() {
		data.life = data.maxLife;
		data.energy = data.maxEnergy;
	}

	public void Death() {
		if(state != Character.State.Ghost)
			GetComponentInChildren<SpriteRenderer>().material.shader = bwShader;
		Animator a = GetComponentInChildren<Animator>();
		a.SetBool("Alive", false);
		state = Character.State.Dead;
		Cleanup();
	}

	public void Cleanup() {
		RemoveAllStatusEffects();
		if(lifeBar != null)
			Destroy(lifeBar.gameObject);
	}

	public void ShowLifeBar(bool show) {
		if(lifeBar != null)
			lifeBar.gameObject.SetActive(show);
		if(statusContainer != null)
			statusContainer.gameObject.SetActive(show);
	}

	public void Update() {
		if (state != Character.State.Dead) {
			if(statusContainer == null)
				statusContainer = GameObject.Instantiate(statusContainerPrefab, GameManager.gm.arena.combatUI.statusContainers, false);
			statusContainer.position = Camera.main.WorldToScreenPoint(headTransform.position + new Vector3(0f,.7f,0f));

			if(state != Character.State.Ghost) {
				if(lifeBar == null)
					lifeBar = GameObject.Instantiate(lifeBarPrefab, GameManager.gm.arena.combatUI.statusContainers, false);
				lifeBar.position = Camera.main.WorldToScreenPoint(headTransform.position + new Vector3(0f,.7f,0f));
			}

			if(targetPointer != null)
				targetPointer.position = Camera.main.WorldToScreenPoint(headTransform.position + new Vector3(0f,1f,0f));
		}
	}

}


