using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Roster : MonoBehaviour {
	public int rosterSize = 10;
	public List<CharacterData> goblins = new List<CharacterData>();
	public Transform goblinPrefab;


	// Use this for initialization
	void Start () {
		
	}

	public void Populate () {
		if(goblins.Count > 0) {
			goblins.Clear();
			Debug.Log(goblins.Count.ToString() + " goblins deleted");
		}
		for(int i=0; i < rosterSize; i++) {
			CharacterData data = new CharacterData();
			RollStats(data);
			goblins.Add(data);
		}
		Debug.Log(rosterSize.ToString() + " goblins created");
	}

	public void RollStats(CharacterData data) {
		//randomize stats here
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
