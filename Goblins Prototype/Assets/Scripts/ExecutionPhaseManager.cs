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
	private bool attackSkipped = false;

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
		attacker.target = GetOpponent(attacker);
		attackSkipped = false;
		//dead characters don't attack
		if(attacker.state == Character.State.Dead) {
			attackSkipped = true;
			Debug.Log("\t" + attacker.data.givenName + " is dead, skipping attack\n");
			curAttacker++;
			if (curAttacker >= attackers.Count)
				state = State.End;
			else
				state = State.Attack;
			NextState();
			yield break;
		}

		//characters with no valid targets don't attack
		if(attacker.target == null) {
			attackSkipped= true;
			Debug.Log("\t" + attacker.data.givenName + " has no target, skipping attack\n");
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
		float timer = attackSkipped ? 0f : timeDelayBetweenAttacks;
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
			
		state = State.Init;
	}

	public Character GetOpponent(Character attacker) {
		List<Character> opponents = isPlayerTurn ? arena.enemies : arena.goblins;
		int index = attacker.combatPosition - 1;
		for(int i=0; i<opponents.Count; i++) {
			int y = (index + i) % opponents.Count;
			Character potentialTarget = opponents[y];
			if(potentialTarget.state != Character.State.Dead)
				return potentialTarget;	
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
		}
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(damage, defender.data);
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
			CharacterDeath(defender);
		else
			a.SetTrigger("Melee Defend");
	}

	public void CharacterDeath(Character c) {
		Debug.Log("\t" + (c.isPlayerCharacter ? "Goblin " :"Enemy ") + c.data.givenName + " DIES!\n");
		c.GetComponentInChildren<SpriteRenderer>().material.shader = c.bwShader;
		Animator a = c.GetComponentInChildren<Animator>();
		a.SetBool("Alive", false);
		c.state = Character.State.Dead;

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

