using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour {

	public List <GoblinCombatPanel> goblinPanels;
	public EnemyCombatPanel enemyPanel;
	public Transform goblinPanelGrid;
	public Button rollButton;
	public Button fightButton;
	public Text roundText;
	public Text stateText;

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
}
