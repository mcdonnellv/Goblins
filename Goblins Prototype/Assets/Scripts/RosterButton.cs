﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RosterButton : MonoBehaviour {

	public Button button;
	public Text nameLabel;
	public Text lifeLabel;
	public Text energyLabel;
	public Text bodyLabel;
	public Text mindLabel;
	public Text spiritLabel;
	public Image iconImage;
	public Roster roster;
	private CharacterData character;
	public bool inParty = false;


	// Use this for initialization
	void Start () {
		
	}

	public void Setup (CharacterData currentCharacter) {
		character = currentCharacter;
		nameLabel.text = character.combatClass.type.ToString();
		lifeLabel.text = "Life: " + character.maxLife.ToString();
		energyLabel.text = "Energy: " + character.maxEnergy.ToString();
		bodyLabel.text = "Body: " + character.body.ToString();
		mindLabel.text = "Mind: " + character.mind.ToString();
		spiritLabel.text = "Spirit :" + character.spirit.ToString();
		SetInPartyStatus();
	}

	void SetInPartyStatus() {
		Image bg = GetComponent<Image>();
		bg.color = inParty ? new Color(.4f,.81f,.58f) : Color.white;
		transform.localScale = new Vector3(1,1,1);
	}
		
	public void Pressed () {
		CharacterDetails characterDetails = roster.characterDetailsPanel.GetComponent<CharacterDetails>();
		characterDetails.AssignCharacter(character);
		roster.characterDetailsPanel.gameObject.SetActive(true);

		if(inParty == false) {
			PartyMemberPanel partyMemberPanel = roster.partyGrid.GetChild(PartyMemberPanel.activePanelIndex).GetComponent<PartyMemberPanel>();
			partyMemberPanel.Setup(character);
		}
		roster.RefreshDisplay();
	}
}
