using System.Collections;
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
	public Transform characterDetailsPanel;

	private CharacterData character;

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
	}


	public void Pressed () {
		CharacterDetails characterDetails = characterDetailsPanel.GetComponent<CharacterDetails>();
		characterDetails.character = character;
		characterDetailsPanel.gameObject.SetActive(true);
	}
}
