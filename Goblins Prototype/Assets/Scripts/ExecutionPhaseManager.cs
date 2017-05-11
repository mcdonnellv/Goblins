using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;


public class ExecutionPhaseManager : MonoBehaviour {
	public Arena arena;
	public Transform ghostPrefab;
	public List<Character> attackers = new List<Character>();
	public List<Character> defenders = new List<Character>();

	public List<Transform> playerClashPt;
	public List<Transform> enemyClashPt;
	public bool isPlayerTurn = true;
	public float timeDelayBetweenAttacks;
	public float timeDelayBetweenTurns;
	public float moveAnnounceTimer;
	public GameObject curtain;
	public GhostDeathStatusEffect ghostDeathEffectPrefab;

	private int curAttackPos;
	private bool attackSkipped = false;
	private bool crit = false;
	public float critModifer = 0f;
	private bool hit = false;
	private OverlayCanvasController occ;

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

	public Character GetCurrentAttacker() { 
		foreach(Character a in attackers)
			if(a.combatPosition == curAttackPos)
				return a; 
		return null;
	}
	public Character GetCurrentDefender() { 
		foreach(Character a in defenders)
			if(a.combatPosition == curAttackPos)
				return a; 
		return null;
	}

	IEnumerator InactiveState () {
		while (state == State.Inactive)
			yield return 0;
		NextState();
	}

	IEnumerator InitState () {
		curAttackPos = 1;
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
		Character attacker = GetCurrentAttacker();
		arena.selectedChar = attacker;
		if(attacker == null) {
			attackSkipped = true;
			state = State.AttackDone;
			NextState();
			yield break;
		}
			
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
			state = State.AttackDone;
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
			occ.ShowCombatText(attacker.headTransform.gameObject, CombatTextType.MoveAnnounce, "Pass");
			float timer = 2f;
			while(timer > 0f) {
				timer-=Time.deltaTime;
				yield return 0;
			}

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
				occ.ShowCombatText(mtm,  CombatTextType.MoveAnnounce, "Not Enough energy");
				state = State.AttackDone;
				NextState();
				yield break;
			}
			else
				attacker.data.energy -= attacker.queuedMove.energyCost;
		}
			
		arena.combatUI.ShowTargetPointer(attacker, 3f);

		//roll for hit
		hit = (attacker.target == null) ? true : arena.cm.RollForHit(attacker.data, attacker.target.data, attacker.queuedMove);

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
		occ.ShowCombatText(arena.combatUI.upperAnnounceMarker, CombatTextType.RoundAnnounce, "CRITICAL HIT!");
		Character attacker = GetCurrentAttacker();
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
		Character attacker = GetCurrentAttacker();
		if(attacker != null) {
			CombatMove move = attacker.queuedMove;
			//if(attacker.state != Character.State.Ghost)
				//attacker.GetComponentInChildren<SpriteRenderer>().material.shader = attacker.bwShader;
				
			if(hit) {
				if(move.displaceOpponent && attacker.target != null && attacker.target.state != Character.State.Dead) {
					Character toDisplace = attacker.target;
					int pos = toDisplace.combatPosition;
					if(pos < 4) {
						Transform newPt = toDisplace.isPlayerCharacter ? arena.playerSpawnSpots[pos] : arena.enemySpawnSpots[pos];
						Character inhabitant = arena.GetTransformCharacter(newPt, false, false);
						if(inhabitant == null) //spot to be displaced to is free, proceed
							arena.MoveCharacterToNewPosition(attacker.target, pos + 1);
					}
				}
			}

			float timer = attackSkipped ? .2f : timeDelayBetweenAttacks;
			while(timer > 0f) {
				timer-=Time.deltaTime;
				yield return 0;
			}
		}

		if(!isPlayerTurn) {
			if(attacker != null) {
				attacker.ShowLifeBar(false);
				if(attacker.state != Character.State.Ghost)
					attacker.GetComponentInChildren<SpriteRenderer>().material.shader = attacker.bwShader;
			}
			Character defender = GetCurrentDefender();
			if(defender != null) {
				defender.ShowLifeBar(false);
				if(defender.state != Character.State.Ghost)
					defender.GetComponentInChildren<SpriteRenderer>().material.shader = defender.bwShader;
			}
		}

		if(!isPlayerTurn) {
			MoveCharactersBack(curAttackPos);
			curAttackPos++;
			if(curAttackPos > 4) {
				state = State.End;
				NextState();
				yield break;
			}
		}

		//now swap attackers and defenders
		isPlayerTurn = !isPlayerTurn;
		List<Character> temp = attackers;
		attackers = defenders;
		defenders = temp;
		state = State.PreAttack;
		NextState();
	}

	IEnumerator EndState () {

		foreach(Character a in attackers) {
			a.ShowLifeBar(true);
			a.transform.localScale = Vector3.one;
		}
		foreach(Character d in defenders) {
			d.ShowLifeBar(true);
			d.transform.localScale = Vector3.one;
		}
		
		arena.combatUI.UnFocusPanels();
		BackToIdle();

		if(!isPlayerTurn) {
			BackToSpawnSpot();
			StartCoroutine(GotoSpawnPositions());
		}

		float timer = 1f;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		camAnimator.Play("CamExecuteExit");

		foreach(Character a in attackers){
			if(a.statusContainer != null) {
				a.statusContainer.BroadcastMessage("OnTurnEnded",  new AttackTurnInfo(a), SendMessageOptions.DontRequireReceiver);
				if(a.data.life <= 0)
					CharacterDeath(a);
			}
		}
		foreach(Character a in defenders){
			if(a.statusContainer != null) {
				a.statusContainer.BroadcastMessage("OnTurnEnded",  new AttackTurnInfo(a), SendMessageOptions.DontRequireReceiver);
				if(a.data.life <= 0)
					CharacterDeath(a);
			}
		}

		timer = timeDelayBetweenTurns;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		attackers.Clear();
		defenders.Clear();
		
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

	public void Setup(bool playerTurn) {
		occ = OverlayCanvasController.instance;
		isPlayerTurn = playerTurn;
		attackers.Clear();
		foreach(Character c in arena.goblins) 
			if(c.state != Character.State.Dead)
				attackers.Add(c);
		defenders.Clear();
		foreach(Character c in arena.enemies) 
			if(c.state != Character.State.Dead)
				defenders.Add(c);
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
		case CombatMove.TargetType.AllyBehind: {
				int posbehind = Mathf.Min(attacker.combatPosition + 1, 4);
				foreach(Character a in attackers)
					if(a.combatPosition == posbehind)
						return a;
				return null;
			}
		}
		return null;
	}

	private void MoveCharactersBack(int pos) {
		List<Character> l = new List<Character>();
		foreach(Character c in attackers)
			if(c.combatPosition == pos)
				l.Add(c);
		foreach(Character c in defenders)
			if(c.combatPosition == pos)
				l.Add(c);

		foreach(Character c in l) {
			float conv = (-c.combatPosition + 5f) * .25f; // .25 to 1
			float s = .6f + (c.combatPosition / 4f * .4f);
			c.transform.Translate(0, conv, conv);
			c.transform.localScale = new Vector3(s,s,s);
		}
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
		int pos = caster.combatPosition;
		if(isPlayerTurn) 
			caster.transform.SetParent(playerClashPt[pos-1].transform, false);
		else 
			caster.transform.SetParent(enemyClashPt[pos-1].transform, false);

		Animator a = isPlayerTurn ? playerClashPt[pos-1].GetComponent<Animator>() : enemyClashPt[pos-1].GetComponent<Animator>();
		if(a != null) 
			a.SetBool("active", true);

		float timer = .34f / 1.5f;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
				
		CombatUI ui = GameManager.gm.arena.combatUI;
		GameObject moveTextMarker = isPlayerTurn ? ui.moveAnnouncePlayerMarker : ui.moveAnnounceEnemyMarker;
		occ.ShowCombatText(moveTextMarker,  CombatTextType.MoveAnnounce, caster.queuedMove.moveName.ToUpper());
		timer = moveAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		CastOnFriendlyCharacter(caster, target);
	}

	public void CastOnFriendlyCharacter(Character caster, Character target) {
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		if(isPlayerTurn)
			camAnimator.Play("CamZoomFriendlyCast");
		else
			camAnimator.Play("CamZoomEnemyCast");

		//add any status effects that may come from the spell
		foreach(BaseStatusEffect se in caster.queuedMove.moveStatusEffects){
			target.AddStatusEffect(se);
			occ.ShowCombatText(target.headTransform.gameObject, CombatTextType.StatusAppliedGood, se.statusEffectName);
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
			occ.ShowCombatText(target.headTransform.gameObject, CombatTextType.Heal, "+" + amountHealed.ToString());
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

		//hop to position
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

		//state the move name
		CombatUI ui = GameManager.gm.arena.combatUI;
		GameObject moveTextMarker = isPlayerTurn ? ui.moveAnnouncePlayerMarker : ui.moveAnnounceEnemyMarker;
		occ.ShowCombatText(moveTextMarker,  CombatTextType.MoveAnnounce, attacker.queuedMove.moveName.ToUpper());

		timer = moveAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		//if attack is melee, dash to a suitable position
		if(attacker.queuedMove.rangeType == CombatMove.RangeType.Melee) {
			GameObject go = attacker.transform.parent.gameObject;
			go.GetComponent<Animator>().enabled = false;
			Vector3 newPos = defender.transform.position + new Vector3(attacker.isPlayerCharacter ? -4f : 4f, 0f, 0f);
			float t = .1f;
			StartCoroutine(GameManager.gm.MoveOverSeconds(go, newPos, t));

			timer = t;
			while(timer > 0f) {
				timer-=Time.deltaTime;
				yield return 0;
			}
		}

		ClashCharacters(attacker, defender);
	}

	IEnumerator ShowDelayedMessage(string message, GameObject go, CombatTextType ctt, float timer) {
		yield return new WaitForSeconds(timer);
		occ.ShowCombatText(go, ctt, message);
	}

	public void ClashCharacters(Character attacker, Character defender) {
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		camAnimator.Play("CamZoomInCombat");
		CombatMove move = attacker.queuedMove;
		move.workingDamage = 0f;
		int finalDamage = 0;
		string damageString = attacker.data.givenName + " misses";
		float fullDamage = 0f;
		if(hit) {
			if(!attacker.queuedMove.isDot) {
				move.workingDamage = arena.cm.RollForDamage(move, attacker, defender);
				if(crit) 
					move.workingDamage *= (arena.cm.baseCritDamageMultiplier + critModifer);
				defender.statusContainer.BroadcastMessage("OnDamageTakenCalc", new AttackTurnInfo(attacker, move.workingDamage), SendMessageOptions.DontRequireReceiver);
				finalDamage = Mathf.FloorToInt(move.workingDamage);
				occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.CriticalHit, crit ? ("Crit\n" + finalDamage.ToString()) : finalDamage.ToString());
				damageString = (crit ? "CRITICAL HIT! " : "") + attacker.data.givenName + "'s " + move.moveName + " deals " + finalDamage.ToString() + " damage to " + defender.data.givenName;

				float resist = arena.cm.GetResistForDamageType(move.damageType, defender.data);
				if(resist > 0)
					StartCoroutine(ShowDelayedMessage(Mathf.FloorToInt(resist * 100f) + "% resisted", defender.headTransform.gameObject, CombatTextType.Miss, 1f));
			}

			//add any status effects that may come from the attack
			foreach(BaseStatusEffect se in move.moveStatusEffects) {
				defender.AddStatusEffect(se);
				occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.StatusAppliedBad, se.statusEffectName);
				defender.statusContainer.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(attacker, se), SendMessageOptions.DontRequireReceiver);
			}
		}
		else
			occ.ShowCombatText(defender.headTransform.gameObject, CombatTextType.Miss, "Miss");
		
		Debug.Log("\t" + damageString + "\n");
		arena.cm.ApplyDamage(finalDamage, defender.data);
		defender.RefreshLifeBar();

		int pos = attacker.combatPosition;

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



	public void CharacterDeath(Character c) {
		if(c.state == Character.State.Dead)
			return;
		Debug.Log("\t" + (c.isPlayerCharacter ? "Goblin " :"Enemy ") + c.data.givenName + " DIES!\n");
		c.Death();
		if(c.isPlayerCharacter) {
			GoblinCombatPanel gcp = arena.combatUI.GetPanelForPlayer(c);
			gcp.GetComponent<CanvasGroup>().alpha = .3f;
			SpawnGhost(c);
		}
	}

	public void SpawnGhost(Character c) {
		GoblinCombatPanel gcp = arena.combatUI.GetPanelForPlayer(c);
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
			if(a != null) {
				a.enabled = true;
				a.SetBool("active", false);
			}
		}

		for( int i=0; i < enemyClashPt.Count; i++) {
			Transform clashPt = enemyClashPt[i];
			Animator a = clashPt.GetComponent<Animator>();
			a.enabled = true;
			if(a != null) {
				a.enabled = true;
				a.SetBool("active", false);
			}
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

