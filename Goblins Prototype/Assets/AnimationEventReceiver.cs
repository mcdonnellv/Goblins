using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationEventReceiver : MonoBehaviour {
	private Character character;

	public void Start() {
		character = transform.parent.GetComponent<Character>();
	}

	public void AttackDone() {
	//	if(character.state != Character.State.Ghost)
	//		character.GetComponentInChildren<SpriteRenderer>().material.shader = character.bwShader;
		GameManager.gm.arena.em.AttackDone();
	}
}
