using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CritFocusRing : CritTargetRing
{
	public float speedMin = 1000;
	public float speedMax = 1500;
	public bool running = false;
	float speed;

	public override void Setup(GameObject go) {
		targetGameObject = go;
		targetRectTransform = targetGameObject.GetComponent<RectTransform>();
		rectTransform.sizeDelta = new Vector2(maxSize, maxSize);
		speed = UnityEngine.Random.Range(speedMin, speedMax);
		running = true;
		UpdatePosition();
	}

	public void Stop() {
		running = false;
		GameManager.gm.arena.combatUI.EndCritGameUI();
	}

	public void Update() {
		if(!running)
			return;
		float size = rectTransform.sizeDelta.x  - (Time.deltaTime * speed);
		rectTransform.sizeDelta = new Vector2(size, size);
		if(rectTransform.rect.width <= 0f)
			Stop();
	}
}

