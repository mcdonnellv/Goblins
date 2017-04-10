using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour {

	public List <GoblinCombatPanel> goblinPanels;
	public Transform goblinPanelGrid;

	public void RefreshPanelPositionNumbers () {
		foreach(Transform child in goblinPanelGrid) {
			GoblinCombatPanel panel = child.GetComponentInChildren<GoblinCombatPanel>();
			panel.position = child.GetSiblingIndex() + 1;
			panel.positionText.text = panel.position.ToString();
		}
	}

	public void StartMoveRoll() {
		float r1 = UnityEngine.Random.Range(1000f, 2000f);
		foreach(GoblinCombatPanel panel in goblinPanels) {
			float r2 = UnityEngine.Random.Range(0f, 1000f * goblinPanels.IndexOf(panel));
			panel.wheel.StartMoveScroll(r1 + r2);
		}
	}
}
