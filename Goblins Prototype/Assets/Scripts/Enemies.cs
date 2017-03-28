using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class StageEnemyList {
	public List<Transform> enemies;
}

public class Enemies : MonoBehaviour {
	public StageEnemyList[] stageEnemySets;
	public List<Transform> enemyParty;


	// Use this for initialization
	void Start () {
		enemyParty = stageEnemySets[0].enemies;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
