using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roster : MonoBehaviour {
	public int rosterSize = 10;
	public List<CharacterData> goblins = new List<CharacterData>();
	public Transform contentPanel;
	public Text rosterCountLabel;
	public SimpleObjectPool buttonObjectPool;
	public List<CombatClass> classes;
	public Transform characterDetailsPanel;
	public Transform partyGrid;
	public CharacterData highlightedCharacter;

	public List<CharacterData> party = new List<CharacterData>();

	// Use this for initialization
	void Start() {
	}

	public void RefreshDisplay() {
		rosterCountLabel.text = goblins.Count.ToString() + "/" + rosterSize.ToString();
		RemoveButtons();
		AddButtons();
	}

	public void RemoveButtons() {
		while(contentPanel.childCount > 0) {
			GameObject toRemove = contentPanel.GetChild(0).gameObject;
			buttonObjectPool.ReturnObject(toRemove);
		}
	}

	public void AddButtons() {
		foreach(CharacterData data in goblins) {
			GameObject newButton = buttonObjectPool.GetObject();
			newButton.transform.SetParent(contentPanel, false);
			RosterButton rosterButton = newButton.GetComponent<RosterButton>();
			rosterButton.inParty = false;
			foreach(Transform child in partyGrid) {
				PartyMemberPanel pmp = child.GetComponent<PartyMemberPanel>();
				if(pmp.character == data) {
					//mark it as part of party
					rosterButton.inParty = true;
					break;
				}
			}
			rosterButton.roster = this;
			rosterButton.Setup(data);
		}
	}

	public void Populate () {
		if(goblins.Count > 0) {
			goblins.Clear();
			Debug.Log(goblins.Count.ToString() + " goblins deleted");
		}
		for(int i=0; i < rosterSize; i++) {
			CharacterData data = new CharacterData();
			data.RollStats();
			goblins.Add(data);

			//assign classes
			if(classes.Count > 1) {
				int randClassIndex = UnityEngine.Random.Range(1, 4); //exclude ghost
				data.AssignClass(classes[randClassIndex]);
			}
		}
			

		//sort by class
		goblins.Sort(
			delegate(CharacterData i1, CharacterData i2) { 
				return i2.maxLife.CompareTo(i1.maxLife); 
				//return i1.combatClass.type.ToString().CompareTo(i2.combatClass.type.ToString()); 
			} 
		);
		Debug.Log(rosterSize.ToString() + " goblins created");
	}
		

	public void CloseButtonPressed() {
		transform.gameObject.SetActive(false);
	}

	public void Highlight(CharacterData character) {
		highlightedCharacter = character; 
		RefreshDisplay();
	}

	// Update is called once per frame
	void Update () {
		
	}
}
