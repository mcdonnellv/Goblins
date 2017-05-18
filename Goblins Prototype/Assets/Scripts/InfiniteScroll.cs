using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour {
	public ScrollRect scroll;        //  Drag PanelB
	public Transform bottomCursor;  // Add empty game object to your scene to define the breakpoint
	public Transform centerCursor;
	// Actually this could be using the screen dimension but I was just doing it quick
	// the x values of those two objects will define when to snap
	public float size = 100f;
	public Transform panelTr;
	private bool rolling = false;

	public void  Movement() {
		if(!rolling)
			return;
		Transform bottom = panelTr.GetChild(panelTr.childCount - 1);    // Get last kid
		if(bottom.position.y < bottomCursor.position.y) {
			bottom.SetAsFirstSibling();
			RectTransform rt = panelTr.GetComponent<RectTransform>();
			Vector2 v = rt.anchoredPosition;
			v.y += size;
			rt.anchoredPosition = v;
		}

		if(scroll.velocity.magnitude <= 50) {
			CenterOnVisibleAndGetMove();
			rolling = false;
		}
	}

	public void CenterOnVisibleAndGetMove() {
		int selIndex = panelTr.childCount - 2;
		Transform child = scroll.content.GetChild(selIndex);
		float normalizePosition = (selIndex * 1f) / (panelTr.childCount * 1f - 1f);
		scroll.verticalNormalizedPosition = 1f - normalizePosition;
		CombatMove cm = child.GetComponentInChildren<WheelEntry>().combatMove;
		GetComponentInParent<GoblinCombatPanel>().SetSelectedMove(cm);
	}

	public void StartMoveScroll(float speed) {
		if(GameManager.gm.useTimeScale) {
			int roll = UnityEngine.Random.Range(0, 3);
			Transform res = scroll.content.GetChild(roll);
			SnapTo(res.GetComponent<RectTransform>(), roll);
			CenterOnVisibleAndGetMove();
		}
		else {
			scroll.velocity = new Vector2(0f, -speed);
			rolling = true;
		}
	}

	public void SnapTo(RectTransform target, int ind) {
		Canvas.ForceUpdateCanvases();
		RectTransform rt = panelTr.GetComponent<RectTransform>();
		rt.anchoredPosition = new Vector2(0, 100 * ind);
	}
}