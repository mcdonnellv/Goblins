using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CritTargetRing : UICircle {

	public float minSize = 200;
	public float maxSize = 600f;
	public float size;
	public RectTransform targetRectTransform;
	public GameObject targetGameObject;
	public Camera mainCamera;

	public virtual void Setup(GameObject go) {
		targetGameObject = go;
		targetRectTransform = targetGameObject.GetComponent<RectTransform>();
		size = UnityEngine.Random.Range(minSize, maxSize);
		rectTransform.sizeDelta = new Vector2(size, size);
		UpdatePosition();
	}

	protected void UpdatePosition() {
		if (targetRectTransform != null)
			transform.position = targetRectTransform.position;
		// If we're targeting a world object, translate our screen position from the world position.
		else
			transform.position = mainCamera.WorldToScreenPoint(targetGameObject.transform.position);
	}
}

