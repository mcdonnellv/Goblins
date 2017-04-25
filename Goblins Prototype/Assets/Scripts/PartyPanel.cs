using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PartyPanel : MonoBehaviour {

	public List<PartyMemberPanel> partyPanels;
	bool ready = false;
	public Button startButton;

	public void StartButtonPressed() {
		gameObject.SetActive(false);
		GameManager.gm.state = GameManager.State.Combat;
		GameManager.gm.roster.party.Clear();
		foreach(PartyMemberPanel p  in partyPanels) {
			GameManager.gm.roster.party.Add(p.character);
			Character.Spawn(p.goblinPrefab, GameManager.gm.arena.playerSpawnSpots[partyPanels.IndexOf(p)], p.character, true);
			if(p.spawnPt.childCount > 0)
				Destroy(p.spawnPt.GetChild(0).gameObject);
			p.character = null;
		}
		
	}

	void Update() {
		if(!ready) {
			//ready if at least one party member is filled
			foreach(PartyMemberPanel p  in partyPanels) {
				if(p.character != null) {
					ready = true;
					break;
				}
			}
		}
		else {
			ready = false;
			foreach(PartyMemberPanel p  in partyPanels) {
				if(p.character != null) {
					ready = true;
					break;
				}
			}
		}

		startButton.gameObject.SetActive(ready);
	}
}
