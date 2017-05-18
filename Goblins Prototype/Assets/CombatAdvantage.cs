using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CombatAdvantage : MonoBehaviour {
	public Text text;
	public Image icon;
	public Image iconBG;
	private Animator animator;
	private CanvasGroup cg;

	public void Show() {
		if(cg == null)
			cg = GetComponent<CanvasGroup>();
		cg.alpha = 1f;
	}

	public void Hide() {
		if(cg == null)
			cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
	}

}
