﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Roster : MonoBehaviour {
	public int rosterSize = 10;
	public List<CharacterData> goblins = new List<CharacterData>();
	public Transform contentPanel;
	public Text rosterCountLabel;
	public SimpleObjectPool buttonObjectPool;
	public Transform goblinPrefab;
	public List<CombatClass> classes;
	public Transform characterDetailsPanel;


	// Use this for initialization
	void Start() {
		if(goblins.Count == 0)
			Populate();
		RefreshDisplay();
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
			rosterButton.Setup(data);
			rosterButton.characterDetailsPanel = characterDetailsPanel;
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
				int randClassIndex = UnityEngine.Random.Range(1, classes.Count);
				data.combatClass = classes[randClassIndex];
			}
		}
			

		//sort by class
		goblins.Sort(
			delegate(CharacterData i1, CharacterData i2) { 
				return i1.combatClass.type.ToString().CompareTo(i2.combatClass.type.ToString()); 
			} 
		);
		Debug.Log(rosterSize.ToString() + " goblins created");
	}
		

	public void SpawnGoblin(int i) {
		Transform goblinTransform = Instantiate(goblinPrefab);
		Character goblin = goblinTransform.GetComponent<Character>();
		goblin.data = goblins[i];
		Debug.Log("Goblin spawned");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}