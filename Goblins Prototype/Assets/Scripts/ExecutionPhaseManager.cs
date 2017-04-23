using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EckTechGames.FloatingCombatText;

public class AttackTurnInfo : Object {
	public Character attacker;
	public Character defender;
	public float damage;
	public BaseStatusEffect statusEffect;

	public AttackTurnInfo(Character a) { attacker = a; }
	public AttackTurnInfo(Character a, float d) { attacker = a; damage = d;}
	public AttackTurnInfo(Character a, BaseStatusEffect s) { attacker = a; statusEffect = s;}
}

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
		attackSkipped = false;
		attacker.BroadcastMessage("OnMyTurnStarted",  new AttackTurnInfo(attacker), SendMessageOptions.DontRequireReceiver);
		//attacker can die from dots
		if(attacker.data.life <= 0) 
			CharacterDeath(attacker);

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

		attacker.target = GetTarget(attacker);
		if(attacker.target != null)
			attacker.target.BroadcastMessage("OnIGotTargetted",  new AttackTurnInfo(attacker), SendMessageOptions.DontRequireReceiver);
		
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
		case CombatMove.TargetType.MostDamagedAlly: {
				Character tar = null;
				List<Character> aliveAllies = new List<Character>();
				Character mosthurt = attacker;
				foreach(Character a in attackers)
					if(a.state != Character.State.Dead && a.data.life != a.data.maxLife && a.data.life < mosthurt.data.life)
						mosthurt = a;
				return mosthurt;
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
		//add any status effects that may come from the spell
		foreach(BaseStatusEffect se in caster.queuedMove.moveStatusEffects){
			target.AddStatusEffect(se);
			OverlayCanvasController.instance.ShowCombatText(target.headTransform.gameObject, CombatTextType.StatusAppliedGood, se.statusEffectName);
			target.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(caster, se), SendMessageOptions.DontRequireReceiver);
		}
		caster.Idle();
		caster.GetComponentInChildren<Animator>().SetTrigger("Ranged Cast");

		if(caster.queuedMove.moveType == CombatMove.MoveType.Heal) {
			float damageHealed = 0f;
			int finalDamageHealed = 0;
			bool crit = false;
			damageHealed = caster.queuedMove.effectiveness;
			crit = arena.cm.RollForCrit(caster.data);
			if(crit)
				damageHealed = damageHealed * arena.cm.baseCritDamage;
			finalDamageHealed = Mathf.FloorToInt(damageHealed);
			int amountHealed = arena.cm.ApplyHeal(finalDamageHealed, target.data);
			occ.ShowCombatText(target.headTransform.gameObject, CombatTextType.Heal, amountHealed);

		}
	}

	public void ClashCharacters(Character attacker, Character defender) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		bool hit = arena.cm.RollForHit(attacker.data, defender.data);
		bool crit = false;
		float damage = 0f;
		int finalDamage = 0;
		string damageString = attacker.data.givenName + " misses";
		if(hit) {
			if(!attacker.queuedMove.isDot) {
				damage = arena.cm.RollForDamage(attacker.queuedMove, attacker, defender);
				crit = arena.cm.RollForCrit(attacker.data);
				if(crit)
					damage = damage * arena.cm.baseCritDamage;

				defender.BroadcastMessage("OnDamageTakenCalc", new AttackTurnInfo(attacker, damage), SendMessageOptions.DontRequireReceiver);
				finalDamage = Mathf.FloorToInt(damage);
				occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.CriticalHit, finalDamage);
				damageString = (crit ? "CRITICAL HIT! " : "") + attacker.data.givenName + "'s " + attacker.queuedMove.moveName + " deals " + damage.ToString() + " damage to " + defender.data.givenName;

				float resist = arena.cm.GetResistForDamageType(attacker.queuedMove.damageType, defender.data);
				if(resist > 0)
					occ.ShowCombatTextDelay(defender.headTransform.gameObject, CombatTextType.Miss, resist.ToString() + " resisted", 1.5f);
			}

			//add any status effects that may come from the attack
			foreach(BaseStatusEffect se in attacker.queuedMove.moveStatusEffects) {
				defender.AddStatusEffect(se);
				OverlayCanvasController.instance.ShowCombatText(defender.headTransform.gameObject, CombatTextType.StatusAppliedBad, se.statusEffectName);
				defender.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(attacker, se), SendMessageOptions.DontRequireReceiver);
			}
		}
		else
			occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.Miss, "Miss");
		
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(finalDamage, defender.data);

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

