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
	public Text energyLabel;
	public Text critRateLabel;
	public Text defenseLabel;
	public Text ageLabel;

	public Text sliceResLabel;
	public Text crushResLabel;
	public Text arcaneResLabel;
	public Text darkResLabel;
	public Text coldResLabel;
	public Text fireResLabel;
	public Image iconImage;
	public Dropdown classDropdown;


	public GameObject movePanelPrefab;
	public Transform movePanelGrid;
	public Roster roster;


	public void CloseButtonPressed() {
		transform.gameObject.SetActive(false);
	}


	// Use this for initialization
	void Start () {
		transform.gameObject.SetActive(false);
	}

	public void AssignCharacter(CharacterData c) {
		character = c;
		Refresh();
	}

	void Refresh () {
		bodyLabel.text = character.body.ToString();
		mindLabel.text = character.mind.ToString();
		spiritLabel.text = character.spirit.ToString();
		lifeLabel.text = character.life.ToString() + "/" + character.maxLife.ToString();
		energyLabel.text = character.energy.ToString() + "/" + character.maxEnergy.ToString();
		critRateLabel.text = (100 * character.critChance).ToString() + "%";
		defenseLabel.text = character.defense.ToString();
		ageLabel.text = character.age.ToString();
		sliceResLabel.text = (100 * character.sliceRes).ToString() + "%";
		crushResLabel.text = (100 * character.crushRes).ToString() + "%";
		arcaneResLabel.text = (100 * character.aracaneRes).ToString() + "%";
		darkResLabel.text = (100 * character.darkRes).ToString() + "%";
		coldResLabel.text = (100 * character.coldRes).ToString() + "%";
		fireResLabel.text = (100 * character.fireRes).ToString() + "%";
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
		character.AssignClass(roster.classes[ind]);
		Refresh();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
