using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Image containerImage;
	private Color normalColor;
	public Color highlightColor = Color.yellow;
	
	public void OnEnable () {
		if (containerImage != null)
			normalColor = containerImage.color;
	}
	
	public void OnDrop(PointerEventData data) {
		containerImage.color = normalColor;
		GameObject dropGameObject = GetDropGameObject(data);
		DragMe dragMe = dropGameObject.GetComponent<DragMe>();
		if(!dragMe.interactable)
			return;

		//reparent current inhabitant
		Transform currentChild = transform.GetChild(0);
		if(currentChild != null) {
			currentChild.SetParent(dragMe.prevParent, false);
			currentChild.SetAsLastSibling();
		}

		//give the droped object a home
		dropGameObject.transform.SetParent(transform, false);
		dropGameObject.transform.SetAsLastSibling();

		//do game logic
		Transform prevChild = currentChild;
		currentChild = dropGameObject.transform;
		GameManager.gm.arena.combatUI.RefreshPanelPositionNumbers();
		GameManager.gm.arena.RepositionGoblins();

	}

	public void OnPointerEnter(PointerEventData data) {
		GameObject dropGameObject = GetDropGameObject(data);
		DragMe dragMe = dropGameObject.GetComponent<DragMe>();
		if(!dragMe.interactable)
			return;
		if (dropGameObject != null)
			containerImage.color = highlightColor;
	}

	public void OnPointerExit(PointerEventData data){
		GameObject dropGameObject = GetDropGameObject(data);
		DragMe dragMe = dropGameObject.GetComponent<DragMe>();
		if(!dragMe.interactable)
			return;
		containerImage.color = normalColor;
	}
	
	private GameObject GetDropGameObject(PointerEventData data) {
		var originalObj = data.pointerDrag;
		if (originalObj == null)
			return null;
		var dragMe = originalObj.GetComponent<DragMe>();
		if (dragMe == null)
			return null;
		return originalObj;
	}
}
