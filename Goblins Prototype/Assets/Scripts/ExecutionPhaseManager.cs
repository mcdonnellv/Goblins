using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
	public Arena arena;
	public Transform ghostPrefab;
	public List<Character> attackers = new List<Character>();
	public List<Transform> playerClashPt;
	public List<Transform> enemyClashPt;
	public bool isPlayerTurn = true;
	public float timeDelayBetweenAttacks;
	public float timeDelayBetweenTurns;
	public float moveAnnounceTimer;
	public GameObject curtain;
	public GhostDeathStatusEffect ghostDeathEffectPrefab;

	private int curAttacker;
	private bool attackSkipped = false;
	private bool crit = false;
	public float critModifer = 0f;
	private bool hit = false;

	public enum State {
		Inactive,
		Init,
		PreAttack,
		CritGame,
		Attack,
		AttackDone,
		End,
	}
	public State state;

	public Character GetCurrentAttacker() { return attackers[curAttacker]; }

	IEnumerator InactiveState () {
		while (state == State.Inactive)
			yield return 0;
		NextState();
	}

	IEnumerator InitState () {
		curAttacker = 0;
		if(attackers.Count == 0)
			state = State.End;
		else
			state = State.PreAttack;
		while (state == State.Init)
			yield return 0;
		NextState();
	}

	IEnumerator PreAttackState () {
		critModifer = 0f;
		if(curAttacker >=  attackers.Count) {
			state = State.End;
			NextState();
			yield break;
		}

		Character attacker = attackers[curAttacker];
		if(isPlayerTurn)
			arena.combatUI.FocusPanel(attacker.combatPosition);
		attackSkipped = false;
		if(attacker.statusContainer != null)
			attacker.statusContainer.BroadcastMessage("OnMyTurnStarted",  new AttackTurnInfo(attacker), SendMessageOptions.DontRequireReceiver);

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
				state = State.PreAttack;
			NextState();
			yield break;
		}
			

		attacker.target = GetTarget(attacker);
		if(attacker.target != null) {
			attacker.target.statusContainer.BroadcastMessage("OnIGotTargetted",  new AttackTurnInfo(attacker), SendMessageOptions.DontRequireReceiver);
			if(attacker.target.isPlayerCharacter && !isPlayerTurn)
				arena.combatUI.FocusPanel(attacker.target.combatPosition);
		}

		//characters with no valid targets don't attack
		if(attacker.target == null) {
			attackSkipped = true;
			Debug.Log("\t" + attacker.data.givenName + " has no target, skipping attack\n");
			state = State.AttackDone;
			NextState();
			yield break;
		}

		//check if we have enough energy to perform the move
		if(isPlayerTurn) {
			if (attacker.queuedMove.energyCost > attacker.data.energy) {
				attackSkipped = true;
				Debug.Log("\t" + attacker.data.givenName + " does not have enough energy to attack, skipping attack\n");
				GameObject mtm = isPlayerTurn ? GameManager.gm.arena.combatUI.moveAnnouncePlayerMarker : GameManager.gm.arena.combatUI.moveAnnounceEnemyMarker;
				OverlayCanvasController.instance.ShowCombatText(mtm,  CombatTextType.MoveAnnounce, "Not Enough energy");
				state = State.AttackDone;
				NextState();
				yield break;
			}
			else
				attacker.data.energy -= attacker.queuedMove.energyCost;
		}
			
		arena.combatUI.ShowTargetPointer(attacker, 3f);

		//roll for hit
		hit = (attacker.target == null) ? true : arena.cm.RollForHit(attacker.data, attacker.target.data);

		//roll for crit
		crit = hit ? arena.cm.RollForCrit(attacker.queuedMove, attacker.data) : false;

		if(isPlayerTurn && crit)
			state = State.CritGame;
		else
			state = State.Attack;

		while (state == State.PreAttack) {
			yield return 0;
		}
		NextState();
	}


	IEnumerator CritGameState () {
		OverlayCanvasController.instance.ShowCombatText(arena.combatUI.upperAnnounceMarker, CombatTextType.RoundAnnounce, "CRITICAL HIT!");
		Character attacker = attackers[curAttacker];
		arena.combatUI.StartCritGameUI(attacker.headTransform.gameObject);
		while (state == State.CritGame) {
			yield return 0;
		}
		NextState();
	}

	IEnumerator AttackState() {
		Character attacker = GetCurrentAttacker();
		if(attacker.queuedMove.targetType == CombatMove.TargetType.Opponent || attacker.queuedMove.targetType == CombatMove.TargetType.RandomOpponent)
			StartCoroutine(GotoClashPositions(attacker, attacker.target));
		else
			StartCoroutine(GotoCastPositions(attacker, attacker.target));

		if(attacker.target != null && !attacker.target.isPlayerCharacter)
			arena.combatUI.ShowEnemyPanel(attacker.target);
		
		while (state == State.Attack)
			yield return 0;	
		NextState();
	}


	IEnumerator AttackDoneState () {
		curtain.SetActive(false);
		arena.combatUI.HideEnemyPanel();
		//do any post attack combat move effects
		CombatMove move = attackers[curAttacker].queuedMove;
		Character attacker = GetCurrentAttacker();
		if(attacker.state != Character.State.Ghost)
			attacker.GetComponentInChildren<SpriteRenderer>().material.shader = attacker.bwShader;
			
		if(hit) {
			if(move.displaceOpponent && attacker.target != null && attacker.target.state != Character.State.Dead) {
				int pos = attacker.target.combatPosition;
				arena.MoveCharacterToNewPosition(attackers[curAttacker].target, pos + 1);
			}
		}

		float timer = attackSkipped ? .2f : timeDelayBetweenAttacks;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		curAttacker++;
		if (curAttacker >= attackers.Count)
			state = State.End;
		else
			state = State.PreAttack;
		while (state == State.AttackDone) {
			yield return 0;
		}
		NextState();
	}

	IEnumerator EndState () {
		for( int i = 0; i < arena.goblins.Count; i++)
			arena.goblins[i].ProcessTurnForStatusEffects();

		for( int i = 0; i < arena.enemies.Count; i++)
			arena.enemies[i].ProcessTurnForStatusEffects();

		arena.combatUI.UnFocusPanels();
		BackToIdle();
		BackToSpawnSpot();
		StartCoroutine(GotoSpawnPositions());
		attackers.Clear();

		if(isPlayerTurn)
			arena.state = Arena.State.EnemyExecutionPhase;
		else
			arena.state = Arena.State.EndOfTurn;
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
			Character c = arena.GetTransformCharacter(spawnPt, false, true);
			if(c != null)
				attackers.Add(c);
		}
			
		state = State.Init;
	}

	public Character GetTarget(Character attacker) {
		switch(attacker.queuedMove.targetType){
		case CombatMove.TargetType.Self: return attacker;
		case CombatMove.TargetType.Opponent: return GetOpponent(attacker);
		case CombatMove.TargetType.RandomOpponent:{
				Character tar = null;
				List<Character> opponents = isPlayerTurn ? arena.enemies : arena.goblins;
				List<Character> aliveOpponents = new List<Character>();
				foreach(Character a in aliveOpponents)
					if(a.state != Character.State.Dead && a.state != Character.State.Ghost)
						aliveOpponents.Add(a);
				if(aliveOpponents.Count > 0) {
					int roll = UnityEngine.Random.Range(0, aliveOpponents.Count);
					tar = aliveOpponents[roll];
				}
				return tar;
			}
		case CombatMove.TargetType.RandomAlly: {
				Character tar = null;
				List<Character> aliveAllies = new List<Character>();
				foreach(Character a in attackers)
					if(a.state != Character.State.Dead && a.state != Character.State.Ghost && a != attacker)
						aliveAllies.Add(a);
				if(aliveAllies.Count > 0) {
					int roll = UnityEngine.Random.Range(0, aliveAllies.Count);
					tar = aliveAllies[roll];
				}
				return tar;
			}
		case CombatMove.TargetType.MostDamagedAlly: {
				Character mosthurt = attacker;
				foreach(Character a in attackers)
					if(a.state != Character.State.Dead && a.state != Character.State.Ghost && a.data.life != a.data.maxLife && a.data.life < mosthurt.data.life)
						mosthurt = a;
				return mosthurt;
			}
		}
		return null;
	}

	public Character GetOpponent(Character attacker) {
		List<Character> opponents = isPlayerTurn ? arena.enemies : arena.goblins;
		foreach(Character c in opponents) {
			if(c.combatPosition == attacker.combatPosition && c.state != Character.State.Dead && c.state != Character.State.Ghost) {
				return c;	
			}
		}
		return null;
	}

	IEnumerator GotoCastPositions(Character caster, Character target) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		CombatUI ui = GameManager.gm.arena.combatUI;
		GameObject moveTextMarker = isPlayerTurn ? ui.moveAnnouncePlayerMarker : ui.moveAnnounceEnemyMarker;
		//Image i = moveTextMarker.GetComponent<Image>();
		//i.canvasRenderer.SetAlpha(0.1f);
		//i.CrossFadeAlpha(.5f,.15f,false);
		occ.ShowCombatText(moveTextMarker,  CombatTextType.MoveAnnounce, caster.queuedMove.moveName.ToUpper());
		float timer = moveAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		//i.canvasRenderer.SetAlpha(0f);
		CastOnFriendlyCharacter(caster, target);
	}

	public void CastOnFriendlyCharacter(Character caster, Character target) {
		OverlayCanvasController occ = OverlayCanvasController.instance;

		//add any status effects that may come from the spell
		foreach(BaseStatusEffect se in caster.queuedMove.moveStatusEffects){
			target.AddStatusEffect(se);
			OverlayCanvasController.instance.ShowCombatText(target.headTransform.gameObject, CombatTextType.StatusAppliedGood, se.statusEffectName);
			target.statusContainer.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(caster, se), SendMessageOptions.DontRequireReceiver);
		}
		caster.Idle();
		caster.GetComponentInChildren<Animator>().SetTrigger("Ranged Cast");

		if(caster.queuedMove.moveType == CombatMove.MoveType.Heal) {
			float damageHealed = 0f;
			int finalDamageHealed = 0;
			damageHealed = caster.queuedMove.effectiveness;
			if(crit)
				damageHealed = damageHealed * (arena.cm.baseCritDamageMultiplier + critModifer);
			finalDamageHealed = Mathf.FloorToInt(damageHealed);
			int amountHealed = arena.cm.ApplyHeal(finalDamageHealed, target.data);
			occ.ShowCombatText(target.headTransform.gameObject, CombatTextType.Heal, amountHealed);
			target.RefreshLifeBar();
		}
	}

	IEnumerator GotoClashPositions(Character attacker, Character defender) {
		int pos = attacker.combatPosition;
		if(isPlayerTurn) {
			attacker.transform.SetParent(playerClashPt[pos-1].transform, false);
			defender.transform.SetParent(enemyClashPt[pos-1].transform, false);

		}
		else {
			attacker.transform.SetParent(enemyClashPt[pos-1].transform, false);
			defender.transform.SetParent(playerClashPt[pos-1].transform, false);
		}

		Animator a = playerClashPt[pos-1].GetComponent<Animator>();
		if(a != null) 
			a.SetBool("active", true);
		a = enemyClashPt[pos-1].GetComponent<Animator>();
		if(a != null) 
			a.SetBool("active", true);
		
		float timer = .34f / 1.5f;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}


		OverlayCanvasController occ = OverlayCanvasController.instance;
		CombatUI ui = GameManager.gm.arena.combatUI;
		GameObject moveTextMarker = isPlayerTurn ? ui.moveAnnouncePlayerMarker : ui.moveAnnounceEnemyMarker;
		//Image i = moveTextMarker.GetComponent<Image>();
		//i.canvasRenderer.SetAlpha(0.1f);
		//i.CrossFadeAlpha(.5f,.15f,false);
		occ.ShowCombatText(moveTextMarker,  CombatTextType.MoveAnnounce, attacker.queuedMove.moveName.ToUpper());
		timer = moveAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		//i.canvasRenderer.SetAlpha(0f);

		ClashCharacters(attacker, defender);
	}

	public void ClashCharacters(Character attacker, Character defender) {
		OverlayCanvasController occ = OverlayCanvasController.instance;
		float damage = 0f;
		int finalDamage = 0;
		string damageString = attacker.data.givenName + " misses";
		if(hit) {
			if(!attacker.queuedMove.isDot) {
				damage = arena.cm.RollForDamage(attacker.queuedMove, attacker, defender);
				if(crit) 
					damage = damage * (arena.cm.baseCritDamageMultiplier + critModifer);
				defender.statusContainer.BroadcastMessage("OnDamageTakenCalc", new AttackTurnInfo(attacker, damage), SendMessageOptions.DontRequireReceiver);
				finalDamage = Mathf.FloorToInt(damage);
				occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.CriticalHit, crit ? ("Crit\n" + finalDamage.ToString()) : finalDamage.ToString());
				damageString = (crit ? "CRITICAL HIT! " : "") + attacker.data.givenName + "'s " + attacker.queuedMove.moveName + " deals " + damage.ToString() + " damage to " + defender.data.givenName;

				float resist = arena.cm.GetResistForDamageType(attacker.queuedMove.damageType, defender.data);
				if(resist > 0)
					occ.ShowCombatTextDelay(defender.headTransform.gameObject, CombatTextType.Miss, resist.ToString() + " resisted", 1.5f);
			}

			//add any status effects that may come from the attack
			foreach(BaseStatusEffect se in attacker.queuedMove.moveStatusEffects) {
				defender.AddStatusEffect(se);
				OverlayCanvasController.instance.ShowCombatText(defender.headTransform.gameObject, CombatTextType.StatusAppliedBad, se.statusEffectName);
				defender.statusContainer.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(attacker, se), SendMessageOptions.DontRequireReceiver);
			}
		}
		else
			occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.Miss, "Miss");
		
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(finalDamage, defender.data);
		defender.RefreshLifeBar();

		int pos = attacker.combatPosition;
		AnimateCamera(pos);

		attacker.Idle();
		defender.Idle();
		//curtain.SetActive(true);

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
		c.Death();

		if(c.isPlayerCharacter) {
			GoblinCombatPanel gcp = arena.combatUI.GetPanelForPlayer(c);
			gcp.GetComponent<CanvasGroup>().alpha = .3f;

			//spawn a ghost
			Character g = Character.Spawn(ghostPrefab, c.spawnSpot, null, true).GetComponent<Character>();
			g.state = Character.State.Ghost;
			g.combatPosition = c.combatPosition;
			g.statusContainer = GameObject.Instantiate(g.statusContainerPrefab, arena.combatUI.statusContainers, false);
			GhostDeathStatusEffect gdse = (GhostDeathStatusEffect)g.AddStatusEffect(ghostDeathEffectPrefab);
			gdse.body = c;
			gcp.character = g;
			int ind = arena.goblins.IndexOf(c);
			arena.goblins[ind] = g;
			c.transform.SetAsLastSibling();
			Debug.Log("\t" + g.data.givenName + " appears!\n");
		}
	}

	public void AttackDone() {
		if(state == State.Attack)
			state = State.AttackDone;
	}

	private void BackToIdle() {
		foreach(Character c in arena.goblins)
			if(c == null)
				Debug.Log("WTF!");
			else
				c.Idle();
		foreach(Character c in arena.enemies) 
			c.Idle();
	}

	IEnumerator GotoSpawnPositions() {
		for( int i=0; i < playerClashPt.Count; i++) {
			Transform clashPt = playerClashPt[i];
			Animator a = clashPt.GetComponent<Animator>();
			if(a != null) 
				a.SetBool("active", false);
		}

		for( int i=0; i < enemyClashPt.Count; i++) {
			Transform clashPt = enemyClashPt[i];
			Animator a = clashPt.GetComponent<Animator>();
			if(a != null) 
				a.SetBool("active", false);
		}


		float timer = .34f / 1.5f;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		BackToSpawnSpot();
	}

	private void BackToSpawnSpot() {
		foreach(Character c in arena.goblins)
			c.GoBackToSpawnSpot();
		foreach(Character c in arena.enemies) 
			c.GoBackToSpawnSpot();
	}
}

