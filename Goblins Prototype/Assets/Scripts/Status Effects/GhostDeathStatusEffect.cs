using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostDeathStatusEffect : BaseStatusEffect {
	public Character body;
	private bool applied = false;

	public override void OnStatusRemoved(AttackTurnInfo ati) {
		KillGhost(ati);
	}

	public override void OnStatusExpired(AttackTurnInfo ati) {
		KillGhost(ati);
	}

	private void KillGhost(AttackTurnInfo ati) {
		if(applied)
			return;
		GoblinCombatPanel gcp = GameManager.gm.arena.combatUI.GetPanelForPlayer(owner);
		int i = GameManager.gm.arena.goblins.IndexOf(owner);
		if(i < 0 || i >= GameManager.gm.arena.goblins.Count) {
			Debug.Log("WTF");
			return;
		}
		else {
			GameManager.gm.arena.goblins[i] = body;
		}

		gcp.character = body;
		Debug.Log("\t" + owner.data.givenName + " fades away\n");
		owner.DeSpawn();
		applied = true;
	}
}