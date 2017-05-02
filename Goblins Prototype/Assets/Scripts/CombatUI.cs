using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class CombatUI : MonoBehaviour {

	public List <GoblinCombatPanel> goblinPanels;
	public CharacterDetails characterDetails;
	public EnemyCombatPanel enemyPanel;
	public Transform goblinPanelGrid;
	public Transform statusContainers;
	public Transform targetPointerContainers;
	public Transform targetPointerPrefab;
	public Button rollButton;
	public Button fightButton;
	public Text roundText;
	public Text stateText;
	public GameObject moveAnnouncePlayerMarker;
	public GameObject moveAnnounceEnemyMarker;
	public GameObject centerAnnounceMarker;
	public GameObject upperAnnounceMarker;

	public GameObject tooltipBox;
	public Text tooltipBoxTitle;
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
			if(panel.character != null) {
				Transform newPt = arena.enemySpawnSpots[panel.position - 1];
				Character inhabitant = arena.GetTransformCharacter(newPt, true, true);
				panel.SetOpponent(inhabitant);
			}
		}
	}

	public void ShowEnemyPanel(Character c) {
		enemyPanel.gameObject.SetActive(true);
		enemyPanel.Setup(c);
	}

	public void HideEnemyPanel() {
		enemyPanel.gameObject.SetActive(false);
	}

	public void StartMoveRoll() {
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
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.curtain.SetActive(panel.position != combatPos);
	}

	public void UnFocusPanels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.curtain.SetActive(false);
	}

	public void RollButtonPressed() {
		GameManager.gm.arena.state = Arena.State.MoveRollPhase;
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

	public void ShowToolTip(string title, string description, float time) {
		StopCoroutine("HideToolTip");
		tooltipBox.SetActive(true);
		tooltipBoxTitle.text = title;
		tooltipBoxDescription.text = description;
		if(time > 0f) {
			StartCoroutine(HideToolTip(time));
		}
	}

	IEnumerator HideToolTip(float t) {
		float time = t;
		while (time > 0f) {
			time -= Time.deltaTime;
			yield return 0;
		}
		tooltipBox.SetActive(false);
	}

	public void ShowTargetPointer(Transform target, float time) {
		StopCoroutine("HideTargetPointer");
		Vector3 pos = Camera.main.WorldToScreenPoint(target.position + new Vector3(0f,.7f,0f));
		Transform tp = GameObject.Instantiate(targetPointerPrefab, targetPointerContainers);
		tp.position = pos;
		if(time > 0f) {
			StartCoroutine(HideTargetPointer(time));
		}
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
}
