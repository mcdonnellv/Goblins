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
			Character.Spawn(p.goblinPrefab, GameManager.gm.arena.playerSpawnSpots[partyPanels.IndexOf(p)], p.character);
		}
		
	}

	void Update() {
		if(!ready) {
			ready = true;
			//not ready if at least one party member is null
			foreach(PartyMemberPanel p  in partyPanels) {
				if(p.character == null) {
					ready = false;
					break;
				}
			}
		}
		else {
			foreach(PartyMemberPanel p  in partyPanels) {
				if(p.character == null) {
					ready = false;
					break;
				}
			}
		}

		startButton.gameObject.SetActive(ready);
	}
}
