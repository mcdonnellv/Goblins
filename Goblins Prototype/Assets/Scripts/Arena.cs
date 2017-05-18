using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EckTechGames.FloatingCombatText;

public class Arena : MonoBehaviour {

	public List<Transform> enemySpawnSpots = new List<Transform>();
	public List<Transform> playerSpawnSpots = new List<Transform>();


	public List<Character> goblins = new List<Character>();
	public List<Character> enemies = new List<Character>();
	public CombatUI combatUI;
	public int round;
	public ExecutionPhaseManager em;
	public CombatMath cm;
	public float encounterStartAnnounceTimer;
	public float roundAnnounceTimer;
	public float victoryAnnounceTimer;
	public MoveRollBonusStatusEffect superRollBonusStatusEffect;
	public MoveRollBonusStatusEffect rollBonusStatusEffect;
	private float generalTimer = 0f;
	private OverlayCanvasController occ;
	public Character selectedChar;
	public int rerolls;

	public enum State {
		Inactive,
		Init,
		WaitForRollPhase,
		MoveRollPhase,
		PositionPhase,
		PlayerExecutionPhase,
		EnemyExecutionPhase,
		EndOfTurn,
	}
	public State state;

	IEnumerator InactiveState () {
		while (state == State.Inactive)
			yield return 0;
		NextState();
	}

	IEnumerator InitState () {
		Debug.Log("***Arena Init State***\n");
		occ = OverlayCanvasController.instance;
		round = 1;
		combatUI.gameObject.SetActive(true);
		goblins.Clear();
		enemies.Clear();
		GetListFromSpawnPts(enemySpawnSpots, enemies);
		GetListFromSpawnPts(playerSpawnSpots, goblins);
		combatUI.rollButton.gameObject.SetActive(false);
		combatUI.fightButton.gameObject.SetActive(false);
		combatUI.DeactivatePanels();
		combatUI.HideVersusPanels();
		combatUI.RefreshRerolls();
		foreach(Character c in goblins) {
			int ind = c.combatPosition -1;
			combatUI.goblinPanels[ind].Setup(c);
		}
		combatUI.RefreshPanelPositionNumbers();
		occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "ENCOUNTER!");
		generalTimer = encounterStartAnnounceTimer;
		while(generalTimer > 0f) {
			generalTimer-=Time.deltaTime;
			yield return 0;
		}
		state = State.WaitForRollPhase;
		NextState();
	}

	IEnumerator WaitForRollPhaseState () {
		Debug.Log("***Arena WaitforRollPhase State***\n");
		occ.ShowCombatText(combatUI.upperAnnounceMarker, CombatTextType.RoundAnnounce, "Rolling Moves");
		combatUI.roundText.text = "Round " + round.ToString();
		//enemy rolls moves
		foreach(Character c in enemies) {
			if(c.state == Character.State.Dead)
				continue;
			int roll = UnityEngine.Random.Range(0, c.data.moves.Count);
			c.queuedMove = c.data.moves[roll];
			Debug.Log("\tEnemy " + c.data.givenName +  " has rolled: " + c.queuedMove.moveName + "\n");
		}
		state = Arena.State.MoveRollPhase;
		NextState();
		yield break;
	}

	IEnumerator MoveRollPhaseState () {
		Debug.Log("***Arena MoveRollPhase State***\n");
		combatUI.rollButton.gameObject.SetActive(false);
		combatUI.fightButton.gameObject.SetActive(false);
		foreach(Character c in goblins) {
			if(c.state == Character.State.Ghost)
				c.queuedMove = c.data.moves[UnityEngine.Random.Range(0, c.data.moves.Count -1)];
			else
				c.queuedMove = null;
		}
		combatUI.RevealWheels();
		combatUI.StartMoveRoll();

		generalTimer = .5f;
		while (state == State.MoveRollPhase)
			yield return 0;
		combatUI.StopCoroutine("ForceEndRoll");
		while(generalTimer > 0f) {
			generalTimer-=Time.deltaTime;
			yield return 0;
		}
		NextState();
	}
		

	IEnumerator PositionPhaseState () {
		Debug.Log("***Arena PositionPhase State***\n");
		occ.ShowCombatText(combatUI.upperAnnounceMarker, CombatTextType.RoundAnnounce, "Position Your Goblins");
		combatUI.fightButton.gameObject.SetActive(true);
		if(rerolls > 0)
			combatUI.rollButton.gameObject.SetActive(true);
		combatUI.ActivatePanels();
		combatUI.DisplayMoves();
		combatUI.HideWheels();

		foreach(GoblinCombatPanel panel in combatUI.goblinPanels)
			panel.GetComponent<DragMe>().interactable = true;

		if(GameManager.gm.autoplay)
			state = Arena.State.PlayerExecutionPhase;

		while (state == State.PositionPhase)
			yield return 0;

		if (state == State.MoveRollPhase) {
			NextState();
			yield break;
		}
		Animator camAnimator = Camera.main.gameObject.GetComponent<Animator>();
		camAnimator.Play("CamExecuteEnter");
		combatUI.fightButton.gameObject.SetActive(false);
		combatUI.rollButton.gameObject.SetActive(false);
		occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.RoundAnnounce, "ROUND " + round.ToString());
		generalTimer = roundAnnounceTimer;
		while(generalTimer > 0f) {
			generalTimer-=Time.deltaTime;
			yield return 0;
		}
		NextState();
	}

	IEnumerator PlayerExecutionPhaseState () {
		Debug.Log("***Arena ExecutionPhase State***\n");

		combatUI.positionIndicator.SetActive(false);
		combatUI.DeactivatePanels();
		em.Setup(true);

		while (em.state != ExecutionPhaseManager.State.Inactive)
			yield return 0;
		RepositionEnemies();
		state = State.EndOfTurn;
		NextState();
	}

	IEnumerator EndOfTurnState () {
		Debug.Log("***Arena End Of Turn State***\n");
		combatUI.positionIndicator.SetActive(true);
		for( int i = 0; i < goblins.Count; i++)
			goblins[i].ProcessTurnForStatusEffects();

		for( int i = 0; i < enemies.Count; i++)
			enemies[i].ProcessTurnForStatusEffects();
		
		combatUI.HideMoves();
		bool allGoblinsDead = AreAllGoblinsDead();
		bool allEnemiesDead = true;
		foreach(Character enemy in enemies) {
			if(enemy.state != Character.State.Dead) {
				allEnemiesDead = false;
				break;
			}
		}

		if(!allGoblinsDead && !allEnemiesDead) {
			//regenerate a bit of energy each turn
			foreach(Character goblin in goblins)
				goblin.data.energy = Mathf.Min(goblin.data.maxEnergy, goblin.data.energy + 1);

			round++;
			state = State.WaitForRollPhase;
			NextState();
			yield break;
		}

		if(allGoblinsDead) {
			combatUI.roundText.text = "DEFEAT";
			occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "DEFEAT!");
			//show defeat banner;
		}

		if(allEnemiesDead) {
			combatUI.roundText.text = "VICTORY";
			occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "VICTORY!");
			//show victory banner;
		}

		generalTimer = victoryAnnounceTimer;
		while(generalTimer > 0f) {
			generalTimer-=Time.deltaTime;
			yield return 0;
		}
		
		GameManager.gm.state = GameManager.State.Result;
		state = State.Inactive;
		while (state == State.EndOfTurn)
			yield return 0;
		NextState();
	}

	public bool AreAllGoblinsDead() {
		foreach(Character goblin in goblins)
			if(goblin.state != Character.State.Dead && goblin.state != Character.State.Ghost)
				return false;
		return true;
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

	private void GetListFromSpawnPts(List<Transform> spawnSpots, List<Character> partyList) {
		int combatPosition = 0;
		foreach(Transform child in spawnSpots) {
			combatPosition++;
			Character c = GetTransformCharacter(child, false, true);
			if(c == null) 
					continue;
			c.combatPosition = combatPosition;
			partyList.Add(c);
		}
	}

	public void RepositionGoblins() {
		foreach(Transform spawnSpot in playerSpawnSpots) {
			Character c = GetTransformCharacter(spawnSpot, false, true);
			GoblinCombatPanel p = GetPanelForGoblin(c);
			int ind = playerSpawnSpots.IndexOf(spawnSpot);
			if(p.position - 1 == ind)
				continue;
			MoveCharacterToNewPosition(c, p.position);
		}
	}

	public bool IsThereValidTargetInLane(int combatPosition) {
		foreach(Character g in goblins) {//check if the gonli in my lane is not a valid target. dead or ghost
			if(g.combatPosition != combatPosition)
				continue;
			// this is my lane
			if(g.state == Character.State.Dead || g.state == Character.State.Ghost)
				return false;
			return true;
		}
		return false;
	}

	public void RepositionEnemies() {

		//reposition enemies with no live goblins in their lane
		foreach(Character enemy in enemies) {
			if(enemy.state == Character.State.Dead)
				continue;

			if(IsThereValidTargetInLane(enemy.combatPosition))
				continue; //we have a target dont move;

			foreach(Transform pt in enemySpawnSpots) {
				Character c = GetTransformCharacter(pt, false, false);
				if(c == enemy)
					continue; // don't consider ourself

				int i = pt.GetSiblingIndex();
				Character goblin = GetTransformCharacter(playerSpawnSpots[i], false, false);

				if(goblin != null && c == null) { //theres a goblin to fight AND no live allies... here go here!
					enemy.combatPosition = i + 1;
					MoveCharacterToNewPosition(enemy, enemy.combatPosition);
					break;
				}
			}
		}
	}


	public GoblinCombatPanel GetPanelForGoblin(Character c){
		foreach(GoblinCombatPanel p in combatUI.goblinPanels)
			if(c == p.character)
				return p;
		return null;
	}

	public void CheckAllGoblinMovesSelected() {
		foreach(Character goblin in goblins) {
			if(goblin.state == Character.State.Dead)
				continue;
			if(goblin.queuedMove == null)
				return;
		}

		//check if all wheel results match for a nice combat bonus
		int move1Count = 0;
		int move2Count = 0;
		int move3Count = 0;
		foreach(Character goblin in goblins) {
			if(goblin.state == Character.State.Dead || goblin.state == Character.State.Ghost)
				continue;
			int i = GetNumberForMove(goblin.data, goblin.queuedMove);
			if(i == 1)
				move1Count++;
			if(i == 2)
				move2Count++;
			if(i == 3)
				move3Count++;
		}
			
		if(move1Count == 4 || move2Count == 4 || move3Count == 4) {
			rerolls += 2;
			occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "4 MATCHES!");
			StartCoroutine(ShowDelayedMessage("+2 reroll token", combatUI.centerAnnounceMarker, CombatTextType.Miss, 2f));
			generalTimer = 3f;
		}
		else if(move1Count == 3 || move2Count == 3 || move3Count == 3) {
			rerolls += 1;
			occ.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "3 MATCHES!");
			StartCoroutine(ShowDelayedMessage("+1 reroll token", combatUI.centerAnnounceMarker, CombatTextType.Miss, 2f));
			generalTimer = 3f;
		}
		combatUI.RefreshRerolls();
		state = State.PositionPhase;
	}

	IEnumerator ShowDelayedMessage(string message, GameObject go, CombatTextType ctt, float timer) {
		yield return new WaitForSeconds(timer);
		occ.ShowCombatText(go, ctt, message);
	}

	private int GetNumberForMove(CharacterData c, CombatMove m) {
		for (int i=0; i< c.moves.Count; i++)
			if(c.moves[i] == m)
				return i+1;
		return 0;
	}
		
	public void MoveCharacterToNewPosition(Character character, int newPos) {
		if(character == null)
			return;
		newPos = Mathf.Min(newPos, 4);
		Transform oldPt = character.transform.parent;
		Transform newPt = character.isPlayerCharacter ? playerSpawnSpots[newPos - 1] : enemySpawnSpots[newPos - 1];

		Character inhabitant = GetTransformCharacter(newPt, true, true);

		if(inhabitant != null) {
			// already has a prior inhabitant, swap positions
			inhabitant.transform.SetParent(oldPt, true);
			inhabitant.combatPosition = character.combatPosition;
			inhabitant.spawnSpot = inhabitant.isPlayerCharacter ? playerSpawnSpots[inhabitant.combatPosition-1] : enemySpawnSpots[inhabitant.combatPosition-1];
			StartCoroutine(GameManager.gm.MoveOverSeconds(inhabitant.gameObject, oldPt.position, .5f));
		}

		character.transform.SetParent(newPt, true);
		character.transform.SetAsFirstSibling();
		character.spawnSpot = newPt;
		character.combatPosition = newPos;
		StartCoroutine(GameManager.gm.MoveOverSeconds(character.gameObject, newPt.position, .5f));

	}


	public Character GetTransformCharacter(Transform t, bool allowDead, bool allowGhost) {
		if(t.childCount == 0)
			return null;
		foreach(Transform child in t) {
			Character c = child.GetComponent<Character>();
			if(c == null) {
				c = GetTransformCharacter(child, allowDead, allowGhost); 
				if(c == null)
					continue;
			}
			
			if(c.state == Character.State.Ghost) {
				if(allowGhost)
					return c;
				else
					continue;
			}

			if(c.state == Character.State.Dead) {
				if(allowDead)
					return c;
				else
					continue;
			}

			return c;
		}
		
		return null;
	}

	public void Update() {
		if(state == State.Inactive)
			return;
		
		if(Input.GetMouseButtonDown(0)) {
			Character enemyCharHit = null;
			Character playerCharHit = null;
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if(hit) {
				Character hitchar = hitInfo.transform.GetComponentInParent<Character>();
				if(hitchar != null) {
					if(!hitchar.isPlayerCharacter)
						enemyCharHit = hitchar;
					else
						playerCharHit = hitchar;
				}
			}

			combatUI.DestoryPointers();

			bool versusPanelShown = false;
			if(enemyCharHit != null) {
				combatUI.ShowVersusPanels(enemyCharHit);
				versusPanelShown = true;
				playerCharHit = combatUI.GetCharacterAtPosition(enemyCharHit.combatPosition, false);
			}
				
			if(playerCharHit != null) {
				if(!versusPanelShown) {
					combatUI.ShowVersusPanels(playerCharHit);
					versusPanelShown = true;
				}
				selectedChar = playerCharHit;
				GoblinCombatPanel gcp = combatUI.GetPanelForPlayer(playerCharHit);
				gcp.Pressed();
			}

			if(playerCharHit == null && enemyCharHit == null) {
				combatUI.HideVersusPanels();
				selectedChar = null;
				foreach(GoblinCombatPanel gcp in combatUI.goblinPanels)
					gcp.SetInHighlightedStatus();
			}
		}
	}
}
