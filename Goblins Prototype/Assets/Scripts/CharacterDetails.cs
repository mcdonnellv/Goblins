using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetails : MonoBehaviour {

	public Button closeButton;
	public CharacterData character;

	public Text bodyLabel;
	public Text mindLabel;
	public Text spiritLabel;
	public Text lifeLabel;
	public Text critRateLabel;
	public Text defenseLabel;
	public Text ageLabel;

	public Image sigil;
	public Text sigilLabel;
	public Text unitLabel;
	public Image unit;
	public Image unitBG;

	public Text sliceResLabel;
	public Text crushResLabel;
	public Text arcaneResLabel;
	public Text darkResLabel;
	public Text coldResLabel;
	public Text fireResLabel;
	public Image iconImage;
	public Dropdown classDropdown;
	public bool canChangeClass;


	public GameObject movePanelPrefab;
	public Transform movePanelGrid;
	public Roster roster;



	public void CloseButtonPressed() {
		transform.gameObject.SetActive(false);
	}


	// Use this for initialization
	void Start () {
	}

	public void AssignCharacter(CharacterData c) {
		character = c;
		Refresh();
	}

	void Refresh () {
		if(canChangeClass == false)
			classDropdown.gameObject.SetActive(false);
		iconImage.sprite = character.combatClass.sprite;
		iconImage.SetNativeSize();
		bodyLabel.text = character.body.ToString();
		mindLabel.text = character.mind.ToString();
		spiritLabel.text = character.spirit.ToString();
		lifeLabel.text = character.life.ToString() + "/" + character.maxLife.ToString();
		critRateLabel.text = (100 * character.critChance).ToString() + "%";
		defenseLabel.text = character.defense.ToString();
		ageLabel.text = character.age.ToString();

		sigil.sprite = Character.SpriteForSigil(character.sigil);
		sigilLabel.text = character.sigil.ToString();
		unit.sprite = Character.SpriteForUnitType(character.unitType);
		unitBG.color = Character.ColorForUnitType(character.unitType);
		unitLabel.text = character.unitType.ToString();

		classDropdown.value = (int) character.combatClass.type;

		foreach (Transform child in movePanelGrid.transform) {
			GameObject.Destroy(child.gameObject);
		}

		foreach(CombatMove cm in character.moves) {
			GameObject spawnedGameObject = (GameObject)GameObject.Instantiate(movePanelPrefab);
			spawnedGameObject.transform.SetParent(movePanelGrid, false);
			MovePanel mp = spawnedGameObject.GetComponent<MovePanel>();
			mp.Setup(cm);
		}


	}


	public void ClassChanged(int ind) {
		CombatClassType newClassType = roster.classes[ind].type;
		if(newClassType == CombatClassType.Ghost)
			return; //changing to ghost not allowed
		
		if(newClassType == character.combatClass.type)
			return;
		character.AssignClass(roster.classes[ind]);
		Refresh();
		roster.RefreshDisplay();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
