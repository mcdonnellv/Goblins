using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberPanel : MonoBehaviour {
	public static int activePanelIndex = -1;

	public Text nameLabel;
	public Text lifeLabel;
	public Text energyLabel;
	public Image iconImage;
	public Image sigil;
	public Image unit;
	public Image unitBG;
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
		sigil.sprite = Character.SpriteForSigil(character.sigil);
		unit.sprite = Character.SpriteForUnitType(character.unitType);
		unitBG.color = Character.ColorForUnitType(character.unitType);
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
		roster.characterDetailsPanel.gameObject.SetActive(false);
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
		PartyMemberPanel.activePanelIndex = -1;
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
		roster.characterDetailsPanel.gameObject.SetActive(false);
		roster.RefreshDisplay();
		SetInHighlightedStatus();
	}

	public void DetailsButtonPressed() {
		activePanelIndex = transform.GetSiblingIndex();
		roster.gameObject.SetActive(true);
		CharacterDetails characterDetails = roster.characterDetailsPanel.GetComponent<CharacterDetails>();
		characterDetails.AssignCharacter(character);
		roster.characterDetailsPanel.gameObject.SetActive(true);
		roster.RefreshDisplay();
		roster.Highlight(character);
	}

}
