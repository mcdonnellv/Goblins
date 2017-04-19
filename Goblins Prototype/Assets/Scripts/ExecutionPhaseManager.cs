using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecutionPhaseManager : MonoBehaviour {
	public List<Character> attackers = new List<Character>();
	public bool isPlayerTurn = true;
	public Arena arena;
	public float timeDelayBetweenAttacks;
	public float timeDelayBetweenTurns;
	private Animator animator;
	private int curAttacker;
	public List<Transform> playerClashPt;
	public List<Transform> enemyClashPt;

	public enum State {
		Inactive,
		Init,
		Attack,
		AttackDone,
		End,
	}
	public State state;

	IEnumerator InactiveState () {
		while (state == State.Inactive)
			yield return 0;
		NextState();
	}

	IEnumerator InitState () {
		curAttacker = 0;
		state = State.Attack;
		while (state == State.Init)
			yield return 0;
		NextState();
	}

	IEnumerator AttackState() {
		Character attacker = attackers[curAttacker];
		if(attacker.target == null) {
			Debug.Log("Skipping attack, no target\n");
			state = State.AttackDone;
			NextState();
			yield break;
		}
		bool friendlyTarget = (isPlayerTurn && attacker.target.isPlayerCharacter) || (!isPlayerTurn && !attacker.target.isPlayerCharacter) ? true : false;
		if(friendlyTarget) {}
		else
			ClashCharacters(attacker, attacker.target);
		while (state == State.Attack)
			yield return 0;	
		NextState();
	}

	IEnumerator AttackDoneState () {
		float timer = timeDelayBetweenAttacks;
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
		BackToIdle();
		BackToSpawnSpot();
		attackers.Clear();

		float timer = timeDelayBetweenTurns;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		if(isPlayerTurn)
			arena.state = Arena.State.EnemyExecutionPhase;
		else
			arena.state = Arena.State.Conclusion;
		state = State.Inactive;
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

	void Start () {
		NextState();
	}

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
		state = State.Init;
	}

	public Character GetOpponent(Character attacker) {
		List<Transform> spawnPts = isPlayerTurn ? arena.enemySpawnSpots : arena.playerSpawnSpots;
		int index = attacker.combatPosition - 1;
		for(int i=0; i<4; i++) {
			int y = (index + i) % 4;
			if(spawnPts[y].childCount > 0)
				return spawnPts[y].GetChild(0).GetComponent<Character>();
		}
		return null;
	}

	public void ClashCharacters(Character attacker, Character defender) {
		bool hit = arena.cm.RollForHit(attacker.data, defender.data);
		bool crit = false;
		int damage = 0;
		string damageString = attacker.data.givenName + " misses";
		if(hit) {
			damage = arena.cm.RollForDamage(attacker.queuedMove, defender.data);
			crit = arena.cm.RollForCrit(attacker.data, defender.data);
			if(crit)
				damage = Mathf.FloorToInt(damage * arena.cm.baseCritDamage);
			damageString = (crit ? "CRITICAL HIT! " : "") + attacker.data.givenName + "'s " + attacker.queuedMove.moveName + " deals " + damage.ToString() + " damage to " + defender.data.givenName;
			//error above, enemies have no queued move (null)
		}
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(damage, defender.data);
		arena.combatUI.stateText.text = damageString;
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		int pos = attacker.combatPosition;
		camAnimator.Play("ZoomInCombat "+ pos.ToString());

		attacker.Idle();
		defender.Idle();

		if(isPlayerTurn) {
			attacker.transform.SetParent(playerClashPt[pos-1].transform, false);
			defender.transform.SetParent(enemyClashPt[pos-1].transform, false);
		}
		else {
			attacker.transform.SetParent(enemyClashPt[pos-1].transform, false);
			defender.transform.SetParent(playerClashPt[pos-1].transform, false);
		}
		Animator a = attacker.GetComponentInChildren<Animator>();
		animator = a;
		a.SetTrigger("Melee Attack");
		a = defender.GetComponentInChildren<Animator>();
		if(defender.data.life <= 0)
			a.SetBool("Alive", false);
		else
			a.SetTrigger("Melee Defend");
	}

	public void AttackDone() {
		if(state == State.Attack)
			state = State.AttackDone;
	}

	private void BackToIdle() {
		foreach(Character c in arena.goblins)
			c.Idle();
		foreach(Character c in arena.enemies) 
			c.Idle();
	}

	private void BackToSpawnSpot() {
		foreach(Character c in arena.goblins)
			c.GoBackToSpawnSpot();
		foreach(Character c in arena.enemies) 
			c.GoBackToSpawnSpot();
	}
}

