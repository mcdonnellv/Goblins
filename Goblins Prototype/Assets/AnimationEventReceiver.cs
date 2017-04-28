using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour {
	private Character character;

	public void Start() {
		character = transform.parent.GetComponent<Character>();
	}

	public void AttackDone() {
		//return to spawn position
		if(character.state != Character.State.Ghost)
			character.GetComponentInChildren<SpriteRenderer>().material.shader = character.bwShader;
		GameManager.gm.arena.em.AttackDone();


		//character.transform.SetParent(character.spawnSpot, true);
		//StartCoroutine(GameManager.gm.MoveOverSeconds(character.gameObject, character.spawnSpot.position, .1f));
	}
}
