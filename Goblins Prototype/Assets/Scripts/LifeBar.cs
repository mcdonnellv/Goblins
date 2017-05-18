using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : MonoBehaviour {
	private Character c;
	public RectTransform rt;
	public Text text;
	private float width;
	public bool showText;
	public bool useAsEnergyBar = false;


	// Use this for initialization
	public void Setup (Character ch) {
		Setup(ch, false);
	}

	public void Setup (Character ch, bool isEnergyBar) {
		useAsEnergyBar = isEnergyBar;
		c = ch;
		width = gameObject.GetComponent<RectTransform>().sizeDelta.x - 2f;
		text.gameObject.SetActive(showText);
	}

	public void Refresh() {
		if (c == null)
			return;
		float curval = useAsEnergyBar ? c.data.energy : c.data.life;
		float totVal = useAsEnergyBar ? c.data.maxEnergy : c.data.maxLife;
		rt.sizeDelta = new Vector2(width * curval/totVal, rt.sizeDelta.y);

		text.gameObject.SetActive(showText);
		if(text == null || showText == false)
			return;
		text.text = curval + " / " + totVal;
	}
}
