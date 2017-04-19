using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class StageEnemyList {
	public int minEnemies = 2;
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

			Transform spawnedEnemy = Character.Spawn(enemyPrefabs[prefabIndex], GameManager.gm.arena.enemySpawnSpots[i], null, false);
			enemies.Add(spawnedEnemy);
		}
	}
}

public class Enemies : MonoBehaviour {
	public StageEnemyList[] stageEnemySets;
	public List<Transform> enemyParty;
	public Text enemyDescription;


	// Use this for initialization
	void Start () {
		//roll for enemies
		for(int i=0; i< stageEnemySets.Length; i++) {
			StageEnemyList list = stageEnemySets[i];
			list.PopulateEnemyList(transform);
		}
		SetParty(0);
	}


	public void SetParty(int index) {
		enemyParty = stageEnemySets[index].enemies;
		enemyDescription.text = DescribeEnemyParty();
	}


	public string DescribeEnemyParty() {
		float[] resistances = new float[6];
		float[] magicRes = new float[4];
		float[] physRes = new float[2];

		foreach(Transform t in enemyParty) {
			CharacterData e = t.GetComponent<Character>().data;
			resistances[0] += e.sliceRes;
			resistances[1] += e.crushRes;
			resistances[2] += e.aracaneRes;
			resistances[3] += e.darkRes;
			resistances[4] += e.coldRes;
			resistances[5] += e.fireRes;
		}

		physRes[0] = resistances[0];
		physRes[1] = resistances[1];
		magicRes[0] = resistances[2];
		magicRes[1] = resistances[3];
		magicRes[2] = resistances[4];
		magicRes[3] = resistances[5];
			
		int largestValInd = 0;
		for(int i=1; i < 6; i++) {
			if(Mathf.Abs(resistances[i]) > Mathf.Abs(resistances[largestValInd]))
				largestValInd = i;
		}

		float genPhysRes = 0f;
		float genMagicRes = 0f;
	
		for(int i=0; i<2; i++)
			genPhysRes += physRes[i];
		genPhysRes /= 2f;

		for(int i=0; i<4; i++)
			genMagicRes += magicRes[i];
		genMagicRes /= 4f;

		if(Mathf.Abs(resistances[largestValInd]) > 0) {
			if(resistances[largestValInd] == genPhysRes)
				return "Enemies are " + (resistances[largestValInd] < 0 ? "vulnerable" : "resistant" ) + " to physical damage.";
			if(resistances[largestValInd] == genMagicRes)
				return "Enemies are " + (resistances[largestValInd] < 0 ? "vulnerable" : "resistant" ) + " to magical damage.";
			
			CombatMove.DamageType d =(CombatMove.DamageType)largestValInd;
			return "Enemies are " + 
				(resistances[largestValInd] < 0 ? "vulnerable" : "resistant" ) +   
				" to " + d.ToString() + " damage.";
		}
		return "Enemies are unremarkable";

	}

	// Update is called once per frame
	void Update () {
		
	}
}
