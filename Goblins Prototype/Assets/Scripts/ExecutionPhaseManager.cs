using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionPhaseManager : MonoBehaviour {
	public List<Character> attackers = new List<Character>();
	public bool isPlayerTurn = true;
	public Arena arena;
	private Animator animator;
	private int curAttacker;

	public enum State {
		Init,
		Attack,
		AttackDone,
		End,
	}
	public State state;

	IEnumerator InitState () {
		Debug.Log("***Arena "+( isPlayerTurn ? "Player" : "Enemy")+"ExecutionPhase Start***\n");
		curAttacker = 0;
		state = State.Attack;
		while (state == State.Init)
			yield return 0;
		NextState();
	}

	IEnumerator AttackState() {
		Character attacker = attackers[curAttacker];
		Debug.Log("***Arena "+( isPlayerTurn ? "Player" : "Enemy")+"ExecutionPhase Attack" + (curAttacker+1).ToString() + " Start***\n");
		if(attacker.target == null) {
			Debug.Log("skipping attack, no target\n");
			state = State.AttackDone;
			NextState();
			yield break;
		}

		bool friendlyTarget = (isPlayerTurn && arena.IsCharacterGoblin(attacker.target)) || (!isPlayerTurn && !arena.IsCharacterGoblin(attacker.target)) ? true : false;

		if(friendlyTarget) {
		}
		else {
			ClashCharacters(attacker, attacker.target);
		}
			
		while (state == State.Attack)
			yield return 0;	
		NextState();
	}

	IEnumerator AttackDoneState () {
		float timer = 5f;
		Debug.Log("***Arena "+( isPlayerTurn ? "Player" : "Enemy")+"ExecutionPhase Attack" + (curAttacker+1).ToString() + " End***\n");

		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		curAttacker++;
		if (curAttacker >= attackers.Count)
			state = State.End;
		else
			state = State.Attack;
		while (state == State.AttackDone) {
			yield return 0;
		}
		NextState();
	}

	IEnumerator EndState () {
		Debug.Log("***Arena "+( isPlayerTurn ? "Player" : "Enemy")+"ExecutionPhase Done***\n");
		while (state == State.End)
			yield return 0;
		NextState();
	}


	void NextState () {
		string methodName = state.ToString() + "State";
		System.Reflection.MethodInfo info =
			GetType().GetMethod(methodName,
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance);
		StartCoroutine((IEnumerator)info.Invoke(this, null));
	}

//	private IEnumerator WaitForAnimation ( Animation animation ) {
//		do {
//			yield return null;
//		} while ( animation.isPlaying );
//	}

	public void Setup(List<Transform> spawnPts, bool playerTurn) {
		isPlayerTurn = playerTurn;
		foreach(Transform spawnPt in spawnPts) {
			if(spawnPt.childCount == 0)
				continue;
			attackers.Add(spawnPt.GetChild(0).GetComponent<Character>());
		}

		// set targets
		foreach(Character c in attackers) {
			c.target = GetOpponent(c);
			c.state = Character.State.WaitingToPerformMove;
		}

		NextState();
	}

	public Character GetOpponent(Character attacker) {
		//TODO if no target check other spots.
		List<Transform> spawnPts = isPlayerTurn ?arena.enemySpawnSpots : arena.playerSpawnSpots;
		int index = attacker.combatPosition - 1;
		if(spawnPts[index].childCount == 0)
			return null;
		return spawnPts[index].GetChild(0).GetComponent<Character>();
	}

	public void ClashCharacters(Character attacker, Character defender) {
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		camAnimator.Play("ZoomInCombat");
		if(isPlayerTurn) {
			attacker.transform.SetParent(arena.playerClashPt.transform, false);
			defender.transform.SetParent(arena.enemyClashPt.transform, false);
		}
		else {
			attacker.transform.SetParent(arena.enemyClashPt.transform, false);
			defender.transform.SetParent(arena.playerClashPt.transform, false);
		}
		Animator a = attacker.GetComponentInChildren<Animator>();
		animator = a;
		a.SetTrigger("Melee Attack");
		a = defender.GetComponentInChildren<Animator>();
		a.SetTrigger("Melee Defend");
	}

	public void ClashDone() {
		state = State.AttackDone;
	}
}

