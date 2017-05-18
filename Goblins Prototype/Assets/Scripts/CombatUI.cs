﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class CombatUI : MonoBehaviour {

	public List <GoblinCombatPanel> goblinPanels;
	public CharacterDetails characterDetails;
	public CombatInfoPanel infoPanelPlayer;
	public CombatInfoPanel infoPanelEnemy;
	public GameObject vsIcon;
	public Transform goblinPanelGrid;
	public Transform statusContainers;
	public Transform targetPointerContainers;
	public Transform targetPointerPrefab;
	public Button rollButton;
	public Button fightButton;
	public Text roundText;
	public Text rerollsText;
	public GameObject moveAnnouncePlayerMarker;
	public GameObject moveAnnounceEnemyMarker;
	public GameObject centerAnnounceMarker;
	public GameObject upperAnnounceMarker;
	public GameObject positionIndicator;

	public GameObject tooltipBox;
	public Text tooltipBoxTitle;
	public Text tooltipBoxTurns;
	public Text tooltipBoxDescription;
	public CritTargetRing targetRing;
	public CritFocusRing focusRing;
	public Button critButton;
	public float critScore;

	public void RefreshPanelPositionNumbers () {
		Arena arena = GameManager.gm.arena;
		foreach(Transform panelTrans in goblinPanelGrid) {
			GoblinCombatPanel panel = panelTrans.GetComponentInChildren<GoblinCombatPanel>();

			int i = panel.transform.parent.GetSiblingIndex();
			switch(i){
			case 0: i=3; break;
			case 1: i=2; break;
			case 2: i=1; break;
			case 3: i=0; break;
			}

			panel.position = i + 1;
			panel.positionText.text = panel.position.ToString();
			if(panel.character != null) 
				panel.SetOpponent(GetCharacterAtPosition(panel.position, true));
		}
	}

	public Character GetCharacterAtPosition(int combatPos, bool enemy) {
		Arena arena = GameManager.gm.arena;
		List<Transform> spawnSpots = enemy ? arena.enemySpawnSpots : arena.playerSpawnSpots;
		Transform newPt = spawnSpots[combatPos - 1];
		Character inhabitant = arena.GetTransformCharacter(newPt, true, true);
		return inhabitant;
	}

	public void ShowVersusPanels(Character c) {
		ShowVersusPanels(c, null);
	}

	public void RefreshRerolls() {
		int rerolls = GameManager.gm.arena.rerolls;
		if(rerolls == 0)
			rerollsText.text = "No Rerolls";
		else
			rerollsText.text = GameManager.gm.arena.rerolls.ToString() + (rerolls == 1 ? " Reroll" : " Rerolls") +  " Left";
	}

	public void ShowVersusPanels(Character c, Character c2) {
		CombatInfoPanel panel = (c.isPlayerCharacter) ? infoPanelPlayer : infoPanelEnemy;
		panel.Setup(c);
		panel.Show();
		Character opposing = c2 == null ? GetCharacterAtPosition(c.combatPosition, false) : c2;
		if(opposing != null) {
			panel.opposingInfoPanel.Setup(opposing);
			panel.opposingInfoPanel.Show();
		}
		vsIcon.SetActive(true);
	}

	public void HideVersusPanels() {
		infoPanelEnemy.Hide();
		infoPanelPlayer.Hide();
		vsIcon.SetActive(false);
	}

	public void DestoryPointers() {
		foreach(Transform child in targetPointerContainers)
			Destroy(child.gameObject);
	}

	public void StartMoveRoll() {
		StartCoroutine(ForceEndRoll());
		float r1 = UnityEngine.Random.Range(1000f, 2000f);
		foreach(GoblinCombatPanel panel in goblinPanels) {
			if(panel.character == null)
				continue;
			if(panel.character.state == Character.State.Dead || panel.character.state == Character.State.Ghost)
				continue;
			float r2 = UnityEngine.Random.Range(0f, 1000f * goblinPanels.IndexOf(panel));
			panel.wheel.StartMoveScroll(r1 + r2);
		}
	}

	IEnumerator ForceEndRoll () {
		//this will ensure that the wheel doesnt hang
		float timer = 8f;
		while(timer > 0f && GameManager.gm.arena.state == Arena.State.MoveRollPhase) {
			timer-=Time.deltaTime;
			yield return 0;
		}

		if(GameManager.gm.arena.state == Arena.State.MoveRollPhase) {
			foreach(GoblinCombatPanel panel in goblinPanels) {
				int roll = UnityEngine.Random.Range(0, 3);
				Transform res = panel.wheel.scroll.content.GetChild(roll);
				panel.wheel.SnapTo(res.GetComponent<RectTransform>(), roll);
				panel.wheel.CenterOnVisibleAndGetMove();
			}
		}
	}

	public void HideWheels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.HideWheel();
	}

	public void RevealWheels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.RevealWheel();
	}

	public void DisplayMoves() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.DisplayMove();
	}

	public void HideMoves() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.moveDetails.SetActive(false);
	}

	public void ActivatePanels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.transform.parent.GetComponent<DropMe>().Activate();
	}

	public void DeactivatePanels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.transform.parent.GetComponent<DropMe>().Deactivate();
	}

	public void FocusPanel(int combatPos) {
		foreach(GoblinCombatPanel panel in goblinPanels) {
			panel.curtain.SetActive(panel.position != combatPos);
			panel.SetInHighlightedStatus();
		}
	}

	public void UnFocusPanels() {
		foreach(GoblinCombatPanel panel in goblinPanels) {
			panel.curtain.SetActive(false);
			panel.SetInHighlightedStatus();
		}
	}

	public void RollButtonPressed() {
		Arena arena = GameManager.gm.arena;
		if(arena.rerolls <=0 )
			return;
		arena.rerolls--;
		arena.state = Arena.State.MoveRollPhase;
		RefreshRerolls();
	}

	public void FightButtonPressed() {
		GameManager.gm.arena.state = Arena.State.PlayerExecutionPhase;
	}

	public GoblinCombatPanel GetPanelForPlayer(Character character) {
		foreach(GoblinCombatPanel panel in goblinPanels)
			if(panel.character == character)
				return panel;
		return null;
	}

	public void StartCritGameUI(GameObject go) {
		focusRing.gameObject.SetActive(true);
		targetRing.gameObject.SetActive(true);
		critButton.gameObject.SetActive(true);
		focusRing.Setup(go);
		targetRing.Setup(go);
		focusRing.CrossFadeAlpha(1, .01f, false);
		targetRing.CrossFadeAlpha(1, .01f, false);

	}

	public void EndCritGameUI() {
		focusRing.running = false;
		ExecutionPhaseManager em = GameManager.gm.arena.em;
		CombatMath cm = GameManager.gm.arena.cm;
		critButton.gameObject.SetActive(false);
		string message = "Good";

		if(GameManager.gm.autoplay) {
			message = "Great!";
			em.critModifer += cm.baseCritDamageMultiplierIncrement;
			OverlayCanvasController.instance.ShowCombatText(focusRing.targetGameObject,  CombatTextType.MoveAnnounce, message);
			StartCoroutine(FadeOutRings());
			return;
		}

		if(focusRing.rectTransform.rect.width <= 0f) {
			message = "Miss!";
			em.critModifer -= cm.baseCritDamageMultiplierIncrement;
		}
		else {
			critScore = Mathf.Abs(focusRing.rectTransform.rect.width - targetRing.rectTransform.rect.width);

			if(critScore <= 100) {
				em.critModifer += cm.baseCritDamageMultiplierIncrement;
				message = "Great!";
			}

			if(critScore <= 30) {
				em.critModifer += cm.baseCritDamageMultiplierIncrement;
				message = "Amazing!";
			}

			if(critScore <= 10) {
				em.critModifer += cm.baseCritDamageMultiplierIncrement;
				message = "Perfect!";
			}
		}
			
		OverlayCanvasController.instance.ShowCombatText(focusRing.targetGameObject,  CombatTextType.MoveAnnounce, message);
		StartCoroutine(FadeOutRings());
	}

	IEnumerator FadeOutRings() {
		focusRing.CrossFadeAlpha(0, .5f, false);
		targetRing.CrossFadeAlpha(0, .5f, false);
		float time = 0.5f;
		while (time > 0f) {
			time -= Time.deltaTime;
			yield return 0;
		}
		focusRing.gameObject.SetActive(false);
		targetRing.gameObject.SetActive(false);
		GameManager.gm.arena.em.state = ExecutionPhaseManager.State.Attack;
	}

	public void ShowToolTip(string title, string description, int turns, float time) {
		StopCoroutine("HideToolTip");
		tooltipBox.SetActive(true);
		tooltipBoxTitle.text = title;
		tooltipBoxDescription.text = description;
		if(turns > 0 && turns < 99)
			tooltipBoxTurns.text = turns.ToString() + (turns == 1 ? " turn": " turns") + " left";
		else
			tooltipBoxTurns.text = "";
		if(time > 0f) 
			StartCoroutine(HideToolTip(time));
	}

	IEnumerator HideToolTip(float t) {
		float time = t;
		while (time > 0f) {
			time -= Time.deltaTime;
			yield return 0;
		}
		tooltipBox.SetActive(false);
	}

	public void ShowTargetPointer(Character character, float time) {
		StopCoroutine("HideTargetPointer");
		Transform tp = GameObject.Instantiate(targetPointerPrefab, targetPointerContainers);
		character.targetPointer = tp;
		if(time > 0f)
			StartCoroutine(HideTargetPointer(time));
	}

	IEnumerator HideTargetPointer(float t) {
		float time = t;
		while (time > 0f) {
			time -= Time.deltaTime;
			yield return 0;
		}

		foreach(Transform child in targetPointerContainers)
			Destroy(child.gameObject);
	}

	public void Update() {
		if(positionIndicator != null && positionIndicator.transform.childCount > 0) {
			for(int i=0; i<4; i++) {
				Transform c = positionIndicator.transform.GetChild(i);
				Arena arena = GameManager.gm.arena;
				Vector3 p = arena.playerSpawnSpots[i].position;
				p.z = 0f;
				c.position = Camera.main.WorldToScreenPoint(p + new Vector3(0f,-.5f,0f));
			}

			for(int i=0; i<4; i++) {
				Transform c = positionIndicator.transform.GetChild(i+4);
				Arena arena = GameManager.gm.arena;
				Vector3 p = arena.enemySpawnSpots[i].position;
				p.z = 0f;
				c.position = Camera.main.WorldToScreenPoint(p + new Vector3(0f,-.5f,0f));
			}

		}
	}
}
