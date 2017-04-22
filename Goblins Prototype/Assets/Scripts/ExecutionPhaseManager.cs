using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EckTechGames.FloatingCombatText;

public class ExecutionPhaseManager : MonoBehaviour {
	public List<Character> attackers = new List<Character>();
	public bool isPlayerTurn = true;
	public Arena arena;
	public float timeDelayBetweenAttacks;
	public float timeDelayBetweenTurns;
	private int curAttacker;
	public List<Transform> playerClashPt;
	public List<Transform> enemyClashPt;
	private bool attackSkipped = false;
	public float moveAnnounceTimer;

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
		attacker.target = GetTarget(attacker);

		foreach(BaseStatusEffect se in attacker.target.data.statusEffects)
			se.OnTargetted(attacker.queuedMove, attacker);
		
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

		GameObject moveTextMarker = isPlayerTurn ? GameManager.gm.arena.combatUI.moveAnnouncePlayerMarker : GameManager.gm.arena.combatUI.moveAnnounceEnemyMarker;
		OverlayCanvasController.instance.ShowCombatText(moveTextMarker,  CombatTextType.MoveAnnounce, attacker.queuedMove.moveName);
		float timer = moveAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
			
		if(attacker.queuedMove.targetType == CombatMove.TargetType.Opponent || attacker.queuedMove.targetType == CombatMove.TargetType.RandomOpponent)
			ClashCharacters(attacker, attacker.target);
		else
			CastOnFriendlyCharacter(attacker, attacker.target);

		while (state == State.Attack)
			yield return 0;	
		NextState();
	}

	IEnumerator AttackDoneState () {
		
		//do any post attack combat move effects
		CombatMove move = attackers[curAttacker].queuedMove;
		Character attacker = attackers[curAttacker];
		if(move.displaceOpponent && attacker.target != null && attacker.target.state != Character.State.Dead) {
			int pos = attacker.target.combatPosition;
			arena.MoveCharacterToNewPosition(attackers[curAttacker].target, pos + 1);
		}

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
		foreach(Character c in arena.goblins)
			c.ProcessTurnForStatusEffects();

		foreach(Character c in arena.enemies)
			c.ProcessTurnForStatusEffects();
		
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

	public Character GetTarget(Character attacker) {
		switch(attacker.queuedMove.targetType){
		case CombatMove.TargetType.Self: return attacker;
		case CombatMove.TargetType.Opponent: return GetOpponent(attacker);
		case CombatMove.TargetType.RandomAlly: {
				Character tar = null;
				List<Character> aliveAllies = new List<Character>();
				foreach(Character a in attackers)
					if(a.state != Character.State.Dead && a != attacker)
						aliveAllies.Add(a);
				if(aliveAllies.Count > 0) {
					int roll = UnityEngine.Random.Range(0, aliveAllies.Count);
					tar = aliveAllies[roll];
				}
				return tar;
			}
		}
		return null;
	}

	public Character GetOpponent(Character attacker) {
		List<Character> opponents = isPlayerTurn ? arena.enemies : arena.goblins;
		foreach(Character c in opponents) {
			if(c.combatPosition == attacker.combatPosition && c.state != Character.State.Dead) {
				return c;	
			}
		}
		return null;
	}

	public void CastOnFriendlyCharacter(Character caster, Character target) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		string text = caster.queuedMove.moveName;

		foreach(BaseStatusEffect se in caster.queuedMove.moveStatusEffects){
			target.AddStatusEffect(se);
			se.OnAdd(caster);
		}

		List<Transform> clashPts = isPlayerTurn ? playerClashPt : enemyClashPt;
		int pos1 = caster.combatPosition;
		int pos2 = target.combatPosition;
		AnimateCamera(pos1);

		caster.Idle();
		caster.transform.SetParent(clashPts[pos1-1].transform, false);
		if(caster != target) {
			target.Idle();
			target.transform.SetParent(clashPts[pos2-1].transform, false);
		}
			
		caster.GetComponentInChildren<Animator>().SetTrigger("Ranged Cast");
		occ.ShowCombatText(target.headTransform.gameObject, CombatTextType.Miss, text);
	}

	public void ClashCharacters(Character attacker, Character defender) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		bool hit = arena.cm.RollForHit(attacker.data, defender.data);
		bool crit = false;
		float damage = 0f;
		int finalDamage = 0;
		string damageString = attacker.data.givenName + " misses";
		if(hit) {
			damage = arena.cm.RollForDamage(attacker.queuedMove, attacker.data, defender.data);
			crit = arena.cm.RollForCrit(attacker.data, defender.data);
			if(crit)
				damage = damage * arena.cm.baseCritDamage;
			int statusEffectCount = defender.data.statusEffects.Count;
			for(int i=0; i < statusEffectCount; i++) {
				BaseStatusEffect se = defender.data.statusEffects[i];
				damage = se.OnDamageTakenCalc(attacker.queuedMove, damage);
				statusEffectCount = defender.data.statusEffects.Count;
			}
			finalDamage = Mathf.FloorToInt(damage);
			occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.Hit, (crit ? "CRIT!\n" : "") + finalDamage.ToString());
			damageString = (crit ? "CRITICAL HIT! " : "") + attacker.data.givenName + "'s " + attacker.queuedMove.moveName + " deals " + damage.ToString() + " damage to " + defender.data.givenName;

			//add any status effects that may come from the attack
			foreach(BaseStatusEffect se in attacker.queuedMove.moveStatusEffects)
				defender.AddStatusEffect(se);
		}
		else
			occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.Miss, "Miss");
		
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(attacker.queuedMove, finalDamage, defender.data);

		int pos = attacker.combatPosition;
		AnimateCamera(pos);

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
		if(attacker.queuedMove.rangeType == CombatMove.RangeType.Melee)
			a.SetTrigger("Melee Attack");
		
		if(attacker.queuedMove.rangeType == CombatMove.RangeType.Ranged)
			a.SetTrigger("Ranged Cast");
		
		a = defender.GetComponentInChildren<Animator>();
		if(defender.data.life <= 0)
			CharacterDeath(defender);
		else
			a.SetTrigger("Melee Defend");
	}

	public void AnimateCamera(int pos) {
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		camAnimator.Play("ZoomInCombat "+ pos.ToString());
	}

	public void CharacterDeath(Character c) {
		Debug.Log("\t" + (c.isPlayerCharacter ? "Goblin " :"Enemy ") + c.data.givenName + " DIES!\n");
		c.GetComponentInChildren<SpriteRenderer>().material.shader = c.bwShader;
		Animator a = c.GetComponentInChildren<Animator>();
		a.SetBool("Alive", false);
		c.state = Character.State.Dead;
		c.RemoveAllStatusEffects();

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

