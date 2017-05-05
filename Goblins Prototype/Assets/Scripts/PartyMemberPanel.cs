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
	public Button detailsButton;
	public GameObject highlight;

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
		iconImage.sprite = character.combatClass.icon;
		nameLabel.text = character.combatClass.type.ToString();
		lifeLabel.text = character.maxLife.ToString();
		energyLabel.text = character.maxEnergy.ToString();
		SpawnGoblin();
		cell.SetActive(true);
		SetInHighlightedStatus();
	}

	public void SetInHighlightedStatus() {
		highlight.SetActive(false);
		if(GameManager.gm.roster.highlightedCharacter != character)
			return;
		highlight.SetActive(true);
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
		roster.Highlight(character);
		addButton.gameObject.SetActive(false);
		removeButton.gameObject.SetActive(true);
		SetInHighlightedStatus();
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
		if(character != null && character.characterGameObject != null) {
			Character c = character.characterGameObject.GetComponent<Character>();
			if(c != null)
				c.DeSpawn();
		}
		
		cell.SetActive(false);
		character = null;
		addButton.gameObject.SetActive(true);
		removeButton.gameObject.SetActive(false);
		cell.SetActive(false);
		SetInHighlightedStatus();
	}

	public void DetailsButtonPressed() {
		roster.gameObject.SetActive(true);
		if(roster.characterDetailsPanel.gameObject.activeSelf) {
			roster.characterDetailsPanel.gameObject.SetActive(false);
		}
		else {
			CharacterDetails characterDetails = roster.characterDetailsPanel.GetComponent<CharacterDetails>();
			characterDetails.AssignCharacter(character);
			roster.characterDetailsPanel.gameObject.SetActive(true);
		}
		roster.RefreshDisplay();
		roster.Highlight(character);
	}

}
