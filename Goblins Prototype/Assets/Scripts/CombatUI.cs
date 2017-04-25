using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using EckTechGames.FloatingCombatText;

public class CombatUI : MonoBehaviour {

	public List <GoblinCombatPanel> goblinPanels;
	public EnemyCombatPanel enemyPanel;
	public Transform goblinPanelGrid;
	public Button rollButton;
	public Button fightButton;
	public Text roundText;
	public Text stateText;
	public GameObject moveAnnouncePlayerMarker;
	public GameObject moveAnnounceEnemyMarker;
	public GameObject centerAnnounceMarker;
	public CritTargetRing targetRing;
	public CritFocusRing focusRing;
	public Button critButton;
	public float critScore;

	public void RefreshPanelPositionNumbers () {
		foreach(Transform child in goblinPanelGrid) {
			GoblinCombatPanel panel = child.GetComponentInChildren<GoblinCombatPanel>();
			panel.position = child.GetSiblingIndex() + 1;
			panel.positionText.text = panel.position.ToString();
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
			float r2 = UnityEngine.Random.Range(0f, 1000f * goblinPanels.IndexOf(panel));
			panel.wheel.StartMoveScroll(r1 + r2);
		}
	}

	public void ActivatePanels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.transform.parent.GetComponent<DropMe>().Activate();
	}

	public void DeactivatePanels() {
		foreach(GoblinCombatPanel panel in goblinPanels)
			panel.transform.parent.GetComponent<DropMe>().Deactivate();
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

		//Pos transform.position = mainCamera.WorldToScreenPoint(go.transform.position);
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
}
