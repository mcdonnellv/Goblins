using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class StageEnemyList {
	public static int minEnemies = 3;
	public static int maxEnemies = 4;
	public List<Transform> enemyPrefabs = new List<Transform>();
	public List<Transform> enemies = new List<Transform>();

	public void PopulateEnemyList(Transform parentTransform, List<Transform> allEnemyPrefabs) {

		if (allEnemyPrefabs.Count == 0) {
			Debug.LogWarning("enemyPrefabs is empty, cannot populate enemies list");
			return;
		}

		enemyPrefabs.Clear();
		int enemyCount = UnityEngine.Random.Range(minEnemies, maxEnemies + 1);	
		for(int i=0; i < enemyCount; i++) {
			//randomly choose enemy prefab from all enemy prefabs
			int prefabIndex = UnityEngine.Random.Range(0, allEnemyPrefabs.Count);
			enemyPrefabs.Add(allEnemyPrefabs[prefabIndex]);
		}
	}

	public void SpawnEnemies() {
		enemies.Clear();
		for(int i=0; i < enemyPrefabs.Count; i++) {
			Transform spawnedEnemy = Character.Spawn(enemyPrefabs[i], GameManager.gm.arena.enemySpawnSpots[i], null, false);
			enemies.Add(spawnedEnemy);
		}
	}
}
