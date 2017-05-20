using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class Enemies : MonoBehaviour {
	public StageEnemyList[] stageEnemySets;
	public List<Transform> enemyParty;
	public Text enemyGroupNumber;
	public GameObject enemyTypeContainer;
	public Image unitTypePrefab;
	public int curPartyIndex = 0;
	public int enemySetsCount;
	public List<Transform> enemyPrefabs;
	public float difficutlyModifier = 1f;


	// Use this for initialization
	public void Setup () {
		// roll for enemies
		stageEnemySets = new StageEnemyList[enemySetsCount];
		for(int i=0; i < stageEnemySets.Length; i++) {
			stageEnemySets[i] = new StageEnemyList();
			stageEnemySets[i].PopulateEnemyList(transform, enemyPrefabs);
		}
		curPartyIndex = 0;
	}


	public void SetAndSpawnParty(int index) {
		if(index >= stageEnemySets.Length) {
			Debug.LogWarning("Index oob, no more enemy parties left to fight\n");
			return;
		}
		stageEnemySets[index].SpawnEnemies();
		enemyParty = stageEnemySets[index].enemies;
		enemyGroupNumber.text = "Next Enemies (" + (index+1).ToString() + "/" + enemySetsCount.ToString() + ")";
		foreach(Transform t in enemyTypeContainer.transform)
			Destroy(t.gameObject);
		
		foreach(Transform t in enemyParty) {
			CharacterData e = t.GetComponent<Character>().data;
			Image unitTypeBG = (Image)Instantiate(unitTypePrefab, enemyTypeContainer.transform, false);
			unitTypeBG.color = Character.ColorForUnitType(e.unitType);
			unitTypeBG.transform.GetChild(0).GetComponent<Image>().sprite = Character.SpriteForUnitType(e.unitType);
		}
	}
}
