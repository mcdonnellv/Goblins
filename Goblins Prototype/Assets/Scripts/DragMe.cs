using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragMe : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
	private Dictionary<int,GameObject> m_DraggingIcons = new Dictionary<int, GameObject>();
	private Dictionary<int, RectTransform> m_DraggingPlanes = new Dictionary<int, RectTransform>();
	public Transform prevParent;
	public bool interactable = false;

	public void OnBeginDrag(PointerEventData eventData){
		if(!interactable)
			return;
		
		var canvas = FindInParents<Canvas>(gameObject);
		if (canvas == null)
			return;

		prevParent = transform.parent;
		m_DraggingIcons[eventData.pointerId] = Instantiate(gameObject);
		//Destroy(m_DraggingIcons[eventData.pointerId].GetComponent<GoblinCombatPanel>());
		//Destroy(m_DraggingIcons[eventData.pointerId].GetComponentInChildren<InfiniteScroll>());

		m_DraggingIcons[eventData.pointerId].transform.SetParent (canvas.transform, false);
		m_DraggingIcons[eventData.pointerId].transform.SetAsLastSibling();
	
		// We want it to be ignored by the event system.
		var group = m_DraggingIcons[eventData.pointerId].GetComponent<CanvasGroup>();
		if(group == null)
			group = m_DraggingIcons[eventData.pointerId].AddComponent<CanvasGroup>();
		group.blocksRaycasts = false;

		var rectTransform = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();
		rectTransform.sizeDelta = transform.GetComponent<RectTransform>().sizeDelta;
		m_DraggingPlanes[eventData.pointerId]  = canvas.transform as RectTransform;
		
		SetDraggedPosition(eventData);
	}

	public void OnDrag(PointerEventData eventData) {
		if(!interactable)
			return;
		if (m_DraggingIcons[eventData.pointerId] != null)
			SetDraggedPosition(eventData);
	}

	private void SetDraggedPosition(PointerEventData eventData) {
		if(!interactable)
			return;
		var rt = m_DraggingIcons[eventData.pointerId].GetComponent<RectTransform>();
		Vector3 globalMousePos;
		if (RectTransformUtility.ScreenPointToWorldPointInRectangle(m_DraggingPlanes[eventData.pointerId], eventData.position, eventData.pressEventCamera, out globalMousePos)) {
			rt.position = globalMousePos;
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		if(!interactable)
			return;
		if (m_DraggingIcons[eventData.pointerId] != null)
			Destroy(m_DraggingIcons[eventData.pointerId]);

		m_DraggingIcons[eventData.pointerId] = null;
	}

	static public T FindInParents<T>(GameObject go) where T : Component
	{
		if (go == null) return null;
		var comp = go.GetComponent<T>();

		if (comp != null)
			return comp;
		
		var t = go.transform.parent;
		while (t != null && comp == null)
		{
			comp = t.gameObject.GetComponent<T>();
			t = t.parent;
		}
		return comp;
	}
}
