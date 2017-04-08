﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InfiniteScroll : MonoBehaviour {
	public ScrollRect scroll;        //  Drag PanelB
	public Transform bottomCursor;  // Add empty game object to your scene to define the breakpoint
	public Transform centerCursor;
	// Actually this could be using the screen dimension but I was just doing it quick
	// the x values of those two objects will define when to snap
	private int count = 0;
	private float size = 0f;
	public Transform panelTr;

	void Start() {
		count = panelTr.childCount - 1; // How many kids do we have?
		size = 180f;//GetButtonSize();   // Here you pass the width of a button
	}

	public void  Movement() {
		Transform top = panelTr.GetChild (0);            // Get first kid
		Transform bottom = panelTr.GetChild(count);    // Get last kid
		if(bottom.position.y < bottomCursor.position.y) {
			bottom.SetAsFirstSibling();
			RectTransform rt = panelTr.GetComponent<RectTransform>();
			Vector2 v = rt.anchoredPosition;
			v.y += size;
			rt.anchoredPosition = v;
		}

		if(scroll.velocity.magnitude <= 100) {
			scroll.verticalNormalizedPosition = 0.5f;
			string indexText = scroll.content.GetChild(1).GetComponentInChildren<Text>().text;
			GetComponentInParent<GoblinCombatPanel>().SetSelectedMove(int.Parse(indexText) - 1);
		}
	}

	public void StartMoveScroll(float speed) {
		scroll.velocity = new Vector2(0f, -speed);
	}
}