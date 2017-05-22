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
	public Image sigil;
	public Image unit;
	public Image unitBG;
	public GameObject highlight;
	public Roster roster;
	public CharacterData character;
	public bool inParty = false;
	private Color originalColor;

	public void Setup (CharacterData currentCharacter) {
		Image bg = GetComponent<Image>();
		if(originalColor.Equals(new Color(0,0,0,0)))
			originalColor = bg.color;
		character = currentCharacter;
		iconImage.sprite = currentCharacter.combatClass.icon;
		nameLabel.text = character.combatClass.type.ToString();
		lifeLabel.text = character.maxLife.ToString();
		bodyLabel.text = "Body: " + character.body.ToString();
		mindLabel.text = "Mind: " + character.mind.ToString();
		spiritLabel.text = "Spirit :" + character.spirit.ToString();
		sigil.sprite = Character.SpriteForSigil(character.sigil);
		unit.sprite = Character.SpriteForUnitType(character.unitType);
		unitBG.color = Character.ColorForUnitType(character.unitType);
		SetInPartyStatus();
		SetInHighlightedStatus();

	}

	void SetInPartyStatus() {
		Image bg = GetComponent<Image>();
		bg.color = inParty ? originalColor * 2.5f : originalColor;
		transform.localScale = new Vector3(1,1,1);
	}

	public void SetInHighlightedStatus() {
		highlight.SetActive(false);
		if(GameManager.gm.roster.highlightedCharacter != character)
			return;
		highlight.SetActive(true);
		foreach(Transform t in roster.partyGrid) {
			PartyMemberPanel pmp = t.GetComponent<PartyMemberPanel>();
			pmp.SetInHighlightedStatus();
		}
	}
		
	public void Pressed () {
		CharacterDetails characterDetails = roster.characterDetailsPanel.GetComponent<CharacterDetails>();
		characterDetails.AssignCharacter(character);
		roster.characterDetailsPanel.gameObject.SetActive(true);

		if(inParty == false && PartyMemberPanel.activePanelIndex >= 0) {
			PartyMemberPanel partyMemberPanel = roster.partyGrid.GetChild(PartyMemberPanel.activePanelIndex).GetComponent<PartyMemberPanel>();
			partyMemberPanel.Setup(character);
		}
		roster.Highlight(character);
		roster.RefreshDisplay();
	}
}
