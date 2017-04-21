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
			int roll = UnityEngine.Random.Range(0, c.data.moves.Count -1);
			c.queuedMove = c.data.moves[roll];
			Debug.Log("\tEnemy " + c.data.givenName +  " has rolled: " + c.queuedMove.moveName + "\n");
		}
		
		foreach(Character c in goblins)
			c.queuedMove = null;
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
		em.Setup(enemySpawnSpots, false);
		while (state == State.EnemyExecutionPhase)
			yield return 0;
		NextState();
	}

	IEnumerator ConclusionState () {
		Debug.Log("***Arena Conclusion State***\n");


		bool allGoblinsDead = true;
		foreach(Character goblin in goblins) {
			if(goblin.state != Character.State.Dead) {
				allGoblinsDead = false;
				break;
			}
		}
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
			
		GameManager.gm.state = GameManager.State.Result;
		state = State.Inactive;
		while (state == State.Conclusion)
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

	private void GetListFromSpawnPts(List<Transform> spawnSpots, List<Character> partyList) {
		int combatPosition = 0;
		foreach(Transform child in spawnSpots) {
			combatPosition++;
			if(child.childCount == 0)
				continue;
			Transform charObj = child.GetChild(0);
			if(charObj == null)
				continue;
			Character c = charObj.GetComponent<Character>();
			c.combatPosition = combatPosition;
			partyList.Add(c);
		}
	}

	public void RepositionGoblins() {
		foreach(Transform spawnSpot in playerSpawnSpots) {
			if(spawnSpot.childCount == 0)
				continue;
			Transform charObj = spawnSpot.GetChild(0);
			Character c = charObj.GetComponent<Character>();
			GoblinCombatPanel p = GetPanelForGoblin(c);
			int ind = playerSpawnSpots.IndexOf(spawnSpot);
			if(p.position - 1 == ind)
				continue;
			MoveCharacterToNewPosition(c, p.position);
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
			if(goblin.queuedMove == null)
				return;
		}
		state = State.PositionPhase;
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
				Character hitchar = hitInfo.transform.GetComponent<Character>();
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
