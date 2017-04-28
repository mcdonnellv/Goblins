using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GhostDeathStatusEffect : BaseStatusEffect {
	public Character body;

	public override void OnStatusRemoved(AttackTurnInfo ati) {
		KillGhost(ati);
	}

	public override void OnStatusExpired(AttackTurnInfo ati) {
		KillGhost(ati);
	}

	private void KillGhost(AttackTurnInfo ati) {
		Character g = GetComponentInParent<Character>();
		GoblinCombatPanel gcp = GameManager.gm.arena.combatUI.GetPanelForPlayer(g);
		int i = GameManager.gm.arena.goblins.IndexOf(g);
		GameManager.gm.arena.goblins[i] = body;
		gcp.character = body;
		Debug.Log("\t" + g.data.givenName + " fades away\n");
		g.DeSpawn();
	}

	private Character CheckForDeadGoblin(Transform pt) {
		foreach(Transform child in pt) {
			Character d = child.GetComponent<Character>();
			if(d == null)
				continue;
			if(d.state == Character.State.Ghost)
				continue;
			return d;
		}
		return null;
	}

}