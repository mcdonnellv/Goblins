using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropMe : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
	public Image containerImage;
	public Color highlightColor = Color.yellow;
	public Color activeColor = Color.blue;
	public Color inactiveColor = Color.gray;
	
	public void OnEnable () {
	}

	public void Activate() {
		containerImage.color = activeColor;
	}

	public void Deactivate() {
		containerImage.color = inactiveColor;
	}
	
	public void OnDrop(PointerEventData data) {
		containerImage.color = activeColor;
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
		if (dropGameObject == null)
			return;
		DragMe dragMe = dropGameObject.GetComponent<DragMe>();
		if(!dragMe.interactable)
			return;
		containerImage.color = highlightColor;
	}

	public void OnPointerExit(PointerEventData data){
		if(GameManager.gm.arena.state == Arena.State.PositionPhase)
			containerImage.color = activeColor;
		else
			containerImage.color = inactiveColor;
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
