using System;
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
	
	// Update is called once per frame
	void Update () {
		
	}
}
