﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour {

	public List<Transform> enemySpawnSpots = new List<Transform>();
	public List<Transform> playerSpawnSpots = new List<Transform>();
	public Transform playerClashPt;
	public Transform enemyClashPt;

	public List<Character> goblins = new List<Character>();
	public List<Character> enemies = new List<Character>();
	public CombatUI combatUI;
	public int round;
	public ExecutionPhaseManager em;

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
		NextState();
	}

	IEnumerator PlayerExecutionPhaseState () {
		Debug.Log("***Arena PlayerExecutionPhase State***\n");
		combatUI.DeactivatePanels();
		combatUI.fightButton.gameObject.SetActive(false);
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
		round++;
		state = State.WaitForRollPhase;
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
			int ind = spawnSpot.GetSiblingIndex();
			if(p.position - 1 == ind)
				continue;
			Transform newSpawnSpot = playerSpawnSpots[p.position - 1];
			if(spawnSpot.childCount > 0) {
				Transform prevInhabitingObj = newSpawnSpot.GetChild(0);
				prevInhabitingObj.SetParent(spawnSpot,true);
				StartCoroutine(GameManager.gm.MoveOverSeconds(prevInhabitingObj.gameObject, spawnSpot.transform.position, .5f));
			}
			charObj.SetParent(newSpawnSpot,true);
			StartCoroutine(GameManager.gm.MoveOverSeconds(charObj.gameObject, newSpawnSpot.transform.position, .5f));
		}
	}


	public GoblinCombatPanel GetPanelForGoblin(Character c){
		foreach(GoblinCombatPanel p in combatUI.goblinPanels)
			if(c == p.character)
				return p;
		return null;
	}

	public void CheckAllGoblinMovesDone() {
		foreach(Character goblin in goblins) {
			if(goblin.queuedMove == null)
				return;
		}
		state = State.PositionPhase;
	}
		



	public bool IsCharacterGoblin (Character c1) {
		foreach(Character c2 in goblins)
			if(c1==c2)
				return true;
		return false;
	}

	public bool IsCharacterEnemy (Character c1) {
		foreach(Character c2 in enemies)
			if(c1==c2)
				return true;
		return false;
	}





	public void Update() {
		if(Input.GetMouseButtonDown(0)) {
			Character enemyCharHit = null;
			RaycastHit hitInfo = new RaycastHit();
			bool hit = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hitInfo);
			if(hit) {
				Character hitchar = hitInfo.transform.GetComponent<Character>();
				if(hitchar != null && IsCharacterEnemy(hitchar))
					enemyCharHit = hitchar;
			}

			if(enemyCharHit != null)
				combatUI.ShowEnemyPanel(enemyCharHit);
			else
				combatUI.HideEnemyPanel();
		}
	}
}