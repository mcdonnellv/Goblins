﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class StageEnemyList {
	public int minEnemies = 1;
	public int maxEnemies = 4;
	public List<Transform> enemies;
	public List<Transform> enemyPrefabs;

	public void PopulateEnemyList(Transform parentTransform) {

		if (enemyPrefabs.Count == 0) {
			Debug.LogWarning("enemyPrefabs is empty, cannot populate enemies list");
			return;
		}

		int enemyCount = UnityEngine.Random.Range(minEnemies, maxEnemies + 1);	
		for(int i=0; i < enemyCount; i++) {
			//randomly choose enemy from prefabs
			int prefabIndex = UnityEngine.Random.Range(0, enemyPrefabs.Count);
			Transform spawnedEnemy = GameObject.Instantiate(enemyPrefabs[prefabIndex]);
			spawnedEnemy.transform.SetParent(parentTransform);
			enemies.Add(spawnedEnemy);
		}
	}

}

public class Enemies : MonoBehaviour {
	public StageEnemyList[] stageEnemySets;
	public List<Transform> enemyParty;


	// Use this for initialization
	void Start () {
		//roll for enemies
		for(int i=0; i< stageEnemySets.Length; i++) {
			StageEnemyList list = stageEnemySets[i];
			list.PopulateEnemyList(transform);
		}
		enemyParty = stageEnemySets[0].enemies;
	}


	public string DescribeEnemyParty() {
		float[] resistances = new float[6];

		foreach(Transform t in enemyParty) {
			CharacterData e = t.GetComponent<Character>().data;
			resistances[0] += e.sliceRes;
			resistances[1] += e.crushRes;
			resistances[2] += e.aracaneRes;
			resistances[3] += e.darkRes;
			resistances[4] += e.coldRes;
			resistances[5] += e.fireRes;
		}

		float largestVal = 0f;
		int largestValInd = 0;
		for(int i=0; i < 6; i++) {
			if(Mathf.Abs(resistances[i]) > largestVal)
				largestValInd = i;
		}

		if(largestVal > 0) {
			CombatMove.DamageType d =(CombatMove.DamageType)largestValInd;
			return "Enemies are " + 
				(resistances[largestValInd] < 0 ? "vulnarable" : "resistant" ) +   
				" to " + d.ToString() + " damage.";
		}
		return "Enemies are unremarkable";

	}

	// Update is called once per frame
	void Update () {
		
	}
}
