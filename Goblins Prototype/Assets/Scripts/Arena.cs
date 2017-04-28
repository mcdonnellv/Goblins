﻿using System.Collections;
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

	public enum State {
		Inactive,
		Init,
		WaitForRollPhase,
		MoveRollPhase,
		PositionPhase,
		PlayerExecutionPhase,
		EnemyExecutionPhase,
		Conclusion,
	}
	public State state;

	IEnumerator InactiveState () {
		while (state == State.Inactive)
			yield return 0;
		NextState();
	}

	IEnumerator InitState () {
		Debug.Log("***Arena Init State***\n");
		round = 1;
		combatUI.gameObject.SetActive(true);
		goblins.Clear();
		enemies.Clear();
		GetListFromSpawnPts(enemySpawnSpots, enemies);
		GetListFromSpawnPts(playerSpawnSpots, goblins);
		combatUI.RefreshPanelPositionNumbers();
		combatUI.rollButton.gameObject.SetActive(false);
		combatUI.fightButton.gameObject.SetActive(false);
		combatUI.DeactivatePanels();
		combatUI.HideEnemyPanel();
		foreach(Character c in goblins) {
			int ind = goblins.IndexOf(c);
			combatUI.goblinPanels[ind].Setup(c);
		}

		OverlayCanvasController.instance.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "ENCOUNTER!");
		float timer = encounterStartAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		state = State.WaitForRollPhase;
		while (state == State.Init)
			yield return 0;
		NextState();
	}

	IEnumerator WaitForRollPhaseState () {
		Debug.Log("***Arena WaitforRollPhase State***\n");
		combatUI.rollButton.gameObject.SetActive(true);
		combatUI.roundText.text = "Round " + round.ToString();
		combatUI.stateText.text = "Move Roll Phase";

		if(GameManager.gm.autoplay)
			state = Arena.State.MoveRollPhase;
			
		while (state == State.WaitForRollPhase)
			yield return 0;
		NextState();
	}

	IEnumerator MoveRollPhaseState () {
		Debug.Log("***Arena MoveRollPhase State***\n");
		combatUI.stateText.text = "";
		combatUI.rollButton.gameObject.SetActive(false);
		foreach(Character c in enemies) {
			if(c.state == Character.State.Dead)
				continue;
			int roll = UnityEngine.Random.Range(0, c.data.moves.Count);
			c.queuedMove = c.data.moves[roll];
			Debug.Log("\tEnemy " + c.data.givenName +  " has rolled: " + c.queuedMove.moveName + "\n");
		}
		
		foreach(Character c in goblins) {
			if(c.state == Character.State.Ghost)
				c.queuedMove = c.data.moves[UnityEngine.Random.Range(0, c.data.moves.Count -1)];
			else
				c.queuedMove = null;
		}
		combatUI.StartMoveRoll();
		
		while (state == State.MoveRollPhase)
			yield return 0;
		NextState();
	}

	IEnumerator PositionPhaseState () {
		Debug.Log("***Arena PositionPhase State***\n");
		combatUI.stateText.text = "Positioning Phase";
		combatUI.fightButton.gameObject.SetActive(true);
		combatUI.ActivatePanels();
		foreach(GoblinCombatPanel panel in combatUI.goblinPanels)
			panel.GetComponent<DragMe>().interactable = true;

		if(GameManager.gm.autoplay)
			state = Arena.State.PlayerExecutionPhase;

		while (state == State.PositionPhase)
			yield return 0;
		combatUI.fightButton.gameObject.SetActive(false);
		OverlayCanvasController.instance.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.RoundAnnounce, "ROUND " + round.ToString());
		float timer = roundAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}
		NextState();
	}

	IEnumerator PlayerExecutionPhaseState () {
		Debug.Log("***Arena PlayerExecutionPhase State***\n");
		combatUI.DeactivatePanels();
		combatUI.stateText.text = "";
		em.Setup(playerSpawnSpots, true);

		while (state == State.PlayerExecutionPhase)
			yield return 0;

		NextState();
	}

	IEnumerator EnemyExecutionPhaseState () {
		Debug.Log("***Arena EnemyExecutionPhase State***\n");
		RepositionEnemies();
		em.Setup(enemySpawnSpots, false);
		while (state == State.EnemyExecutionPhase)
			yield return 0;
		NextState();
	}

	IEnumerator ConclusionState () {
		Debug.Log("***Arena Conclusion State***\n");
		bool allGoblinsDead = AreAllGoblinsDead();
		bool allEnemiesDead = true;
		foreach(Character enemy in enemies) {
			if(enemy.state != Character.State.Dead) {
				allEnemiesDead = false;
				break;
			}
		}

		if(!allGoblinsDead && !allEnemiesDead) {
			round++;
			state = State.WaitForRollPhase;
			NextState();
			yield break;
		}

		if(allGoblinsDead) {
			combatUI.roundText.text = "DEFEAT";
			OverlayCanvasController.instance.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "DEFEAT!");
			//show defeat banner;
		}

		if(allEnemiesDead) {
			combatUI.roundText.text = "VICTORY";
			OverlayCanvasController.instance.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, "VICTORY!");
			//show victory banner;
		}

		float timer = victoryAnnounceTimer;
		while(timer > 0f) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		for( int i = 0; i < goblins.Count; i++)
			goblins[i].RemoveAllStatusEffects();

		for( int i = 0; i < enemies.Count; i++)
			enemies[i].RemoveAllStatusEffects();
		
		GameManager.gm.state = GameManager.State.Result;
		state = State.Inactive;
		while (state == State.Conclusion)
			yield return 0;
		NextState();
	}

	public bool AreAllGoblinsDead() {
		foreach(Character goblin in goblins) {
			if(goblin.state != Character.State.Dead && goblin.state != Character.State.Ghost) {
				return false;
				break;
			}
		}
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
			if(child.childCount == 0)
				continue;

			Character c = null;
			foreach(Transform charObj in child) {
				if(charObj == null)
					continue;
				c = charObj.GetComponent<Character>();
				if(c == null || c.state == Character.State.Dead)
					continue;
			}

			if(c == null)
				continue;
			
			c.combatPosition = combatPosition;
			partyList.Add(c);
		}
	}

	public void RepositionGoblins() {
		foreach(Transform spawnSpot in playerSpawnSpots) {
			if(spawnSpot.childCount == 0)
				continue;
			Transform charObj = spawnSpot.GetChild(0);
			foreach(Transform child in spawnSpot) {
				if(child.GetComponent<Character>().state == Character.State.Ghost) {
					charObj = child;
					break;
				}
			}
				
			Character c = charObj.GetComponent<Character>();
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

			int moveToPos = -1;
			foreach(Transform pt in enemySpawnSpots) {
				int i = pt.GetSiblingIndex();
				if(i == enemy.combatPosition - 1)
					continue; // don't consider our current pos
				if(playerSpawnSpots[i].childCount == 0)
					continue; //dont consider lanes with no goblins in them
				if(IsThereValidTargetInLane(i+1) == false)
					continue; //dont consider lanes with dead or ghost goblins in them

				if(pt.childCount == 0) { //no allies here go here!
					moveToPos = i+1;
					break;
				}
				else {
					//there's an ally... but are any in this spot alive?
					bool someoneIsHereAndAlive = false;
					foreach(Transform charObj in pt) {
						Character ally = charObj.GetComponent<Character>();
						if(ally == null)
							continue;
						if(ally.state != Character.State.Dead) {
							someoneIsHereAndAlive = true;
							break;
						}
					}

					if(someoneIsHereAndAlive)
						continue;
						
					// no ones here! lets go here.
					moveToPos = i+1;
					break;
				}
			}

			if(moveToPos != -1) {
				enemy.combatPosition = moveToPos;
				MoveCharacterToNewPosition(enemy, enemy.combatPosition);
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

		MoveRollBonusStatusEffect se = null;
		string text1 = "4 MATCHES!!!";
		string text2 = "Super Attack Bonus";
		if(move1Count == 4 || move2Count == 4 || move3Count == 4)
			se = superRollBonusStatusEffect;
		else if(move1Count == 3 || move2Count == 3 || move3Count == 3) {
			se = rollBonusStatusEffect;
			text1 = "3 MATCHES";
			text2 = "Attack Bonus";
		}
		
		if(se != null) {
			OverlayCanvasController.instance.ShowCombatText(combatUI.centerAnnounceMarker, CombatTextType.EncounterStart, text1);
			OverlayCanvasController.instance.ShowCombatTextDelay(combatUI.centerAnnounceMarker, CombatTextType.RoundAnnounce, text2, 1.5f);
			foreach(Character goblin in goblins) {
				goblin.AddStatusEffect(se);
				OverlayCanvasController.instance.ShowCombatText(goblin.headTransform.gameObject, CombatTextType.StatusAppliedGood, se.statusEffectName);
				goblin.BroadcastMessage("OnStatusEffectAddedToMe", new AttackTurnInfo(goblin, se), SendMessageOptions.DontRequireReceiver);
			}
		}

		state = State.PositionPhase;
	}

	private int GetNumberForMove(CharacterData c, CombatMove m) {
		for (int i=0; i< c.moves.Count; i++)
			if(c.moves[i] == m)
				return i+1;
		return 0;
	}
		
	public void MoveCharacterToNewPosition(Character character, int newPos) {
		newPos = Mathf.Min(newPos, 4);
		Transform oldPt = character.transform.parent;
		Transform newPt = character.isPlayerCharacter ? playerSpawnSpots[newPos - 1] : enemySpawnSpots[newPos - 1];
		if(newPt.childCount > 0) {
			// already has a prior inhabitant, swap positions
			Character inhabitant = newPt.GetChild(0).GetComponent<Character>();
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

	public void Update() {
		if(Input.GetMouseButtonDown(0)) {
			Character enemyCharHit = null;
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if(hit) {
				Character hitchar = hitInfo.transform.GetComponentInParent<Character>();
				if(hitchar != null && !hitchar.isPlayerCharacter)
					enemyCharHit = hitchar;
			}

			if(enemyCharHit != null)
				combatUI.ShowEnemyPanel(enemyCharHit);
			else
				combatUI.HideEnemyPanel();
		}
	}
}
