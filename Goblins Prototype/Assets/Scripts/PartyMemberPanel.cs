using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberPanel : MonoBehaviour {
	public static int activePanelIndex = 0;

	public Text nameLabel;
	public Text lifeLabel;
	public Text energyLabel;
	public Image iconImage;

	public CharacterData character;
	public Roster roster;

	public GameObject cell;
	public Button addButton;
	public Button removeButton;

	public Transform spawnPt;
	public Transform goblinPrefab;

	void Start () {
		character = null;
		cell.SetActive(false);
	}

	public void Setup (CharacterData characterParam) {
		if(character != null) {
			if(character.characterGameObject != null) {
				Character c = character.characterGameObject.GetComponent<Character>();
				c.DeSpawn();
				character = null;
			}
		}

		character = characterParam;
		nameLabel.text = character.combatClass.type.ToString();
		lifeLabel.text = "Life: " + character.maxLife.ToString();
		energyLabel.text = "Energy: " + character.maxEnergy.ToString();
		SpawnGoblin();
		cell.SetActive(true);
	}

	public void AddButtonPressed() {
		activePanelIndex = transform.GetSiblingIndex();

		//assign a goblin but check first, it might be already in another party panel.
		foreach(CharacterData goblin in roster.goblins) {
			if(IsGoblinInParty(goblin) == false) {
				Setup(goblin);
				break;
			}
		}
		roster.gameObject.SetActive(true);
		roster.RefreshDisplay();
		addButton.gameObject.SetActive(false);
		removeButton.gameObject.SetActive(true);
	}

	private void SpawnGoblin() {
		//spawn the dude
		if(character.characterGameObject != null) {
			Character c = character.characterGameObject.GetComponent<Character>();
			if(c != null)
				c.DeSpawn();
		}
		Character.Spawn(goblinPrefab, spawnPt, character, true);
	}

	bool IsGoblinInParty(CharacterData goblin) {
		foreach(Transform child in transform.parent) {
			if(child == transform)
				continue;
			if(goblin == child.GetComponent<PartyMemberPanel>().character)
				return true;
		}
		return false;
	}

	public void RemoveButtonPressed() {
		if(character.characterGameObject != null) {
			Character c = character.characterGameObject.GetComponent<Character>();
			if(c != null)
				c.DeSpawn();
		}
		
		cell.SetActive(false);
		character = null;
		addButton.gameObject.SetActive(true);
		removeButton.gameObject.SetActive(false);
		cell.SetActive(false);
	}

}
