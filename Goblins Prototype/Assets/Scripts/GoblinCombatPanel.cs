using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoblinCombatPanel : MonoBehaviour {
	public int position;
	public Text classText;
	public Image unitTypeBg;
	public Image unitTypeIcon;
	public Image sigilIcon;
	public GameObject curtain;
	public GameObject wheelCover;
	public LifeBar lifeBar;
	public Image iconImage;
	public Character character;
	public Character opponent;
	public InfiniteScroll wheel;
	public WheelEntry wheelEntryPrefab;
	public CharacterDetails characterDetails;
	public GameObject moveDetails;
	public Image moveWheelIcon;
	public Text moveNameText;
	public Text moveDescriptionText;
	public GameObject highlight;
	public GameObject rollButton;


	public void Setup(Character c) {
		character = c;
		List<Transform> childrenToDelete = new List<Transform>();
		foreach (Transform child in wheel.scroll.content.transform) 
			childrenToDelete.Add(child);

		foreach (Transform child in childrenToDelete) {
			child.parent = GameManager.gm.transform;
			Destroy(child.gameObject);
		}

		lifeBar.Setup(c);
		iconImage.sprite = character.data.combatClass.icon;
		classText.text = character.data.combatClass.type.ToString().ToUpper();
		unitTypeIcon.sprite = Character.SpriteForUnitType(c.data.unitType);
		unitTypeBg.color = Character.ColorForUnitType(c.data.unitType);
		sigilIcon.sprite = Character.SpriteForSigil(c.data.sigil);
		RefreshWheelEntries();
		RefreshBars();
		GetComponent<CanvasGroup>().alpha = 1f;
		SetOpponent(null);
		moveDetails.SetActive(false);
		rollButton.SetActive(false);
		curtain.SetActive(false);
		HideWheel();
		SetInHighlightedStatus();

	}

	public void SetInHighlightedStatus() {
		highlight.SetActive(false);
		if(GameManager.gm.arena.selectedChar != character)
			return;
		highlight.SetActive(true);
	}


	public void RefreshBars() {
		lifeBar.Refresh();
	}

	void RefreshWheelEntries() {
		foreach(Transform child in wheel.panelTr)
			GameObject.Destroy(child.gameObject);

		foreach(CombatMove cm in character.data.moves) {
			if(cm.moveCategory == CombatMove.MoveCategory.Attack)
				for(int i=0 ; i < character.data.attackWeight; i++)
					AddWheelEntry(cm);

			if(cm.moveCategory == CombatMove.MoveCategory.Defense)
				for(int i=0 ; i < character.data.defendWeight; i++)
					AddWheelEntry(cm);

			if(cm.moveCategory == CombatMove.MoveCategory.Special)
				for(int i=0 ; i < character.data.specialWeight; i++)
					AddWheelEntry(cm);
		}
	}

	public void AddWheelEntry(CombatMove cm) {
		WheelEntry we = Instantiate(wheelEntryPrefab, wheel.panelTr, false);
		we.icon.sprite = CombatMove.SpriteForMoveCategory(cm.moveCategory);
		we.icon.color = CombatMove.ColorForMoveCategory(cm.moveCategory);
		we.name = cm.moveCategory.ToString();
		we.combatMove = cm;
		wheel.scroll.content.sizeDelta = new Vector2(wheel.scroll.content.sizeDelta.x, 100f * wheel.scroll.content.transform.childCount);
		int randpos = UnityEngine.Random.Range(0, wheel.panelTr.childCount);
		we.transform.SetSiblingIndex(randpos);
	}

	public void SetSelectedMove(CombatMove cm) {
		character.queuedMove = cm;
		Debug.Log("\tGoblin " + character.data.givenName +  " has rolled: " + character.queuedMove.moveName + "\n");
		GameManager.gm.arena.CheckAllGoblinMovesSelected();
	}

	public void SetOpponent(Character o) {
		opponent = o;
		if(o == null || o.state == Character.State.Dead)
			return;
	}

	public void Pressed() {
		if(GameManager.gm.arena.selectedChar == null)
			GameManager.gm.arena.selectedChar = character;
		CombatUI ui = GameManager.gm.arena.combatUI;
		ui.DestoryPointers();

		if(opponent != null)
			ui.ShowVersusPanels(opponent);
		
		foreach(GoblinCombatPanel gcp in ui.goblinPanels)
			gcp.SetInHighlightedStatus();
	}

	public void HideWheel() {
		wheelCover.GetComponent<Animator>().SetBool("revealed", false);
	}
	public void RevealWheel() {
		moveDetails.SetActive(false);
		wheelCover.GetComponent<Animator>().SetBool("revealed", true);
	}

	public void DisplayMove() {
		if(character == null || character.queuedMove == null)
			return;
		CombatMove combatMove = character.queuedMove;
		moveDetails.SetActive(true);
		moveNameText.text = combatMove.moveName.ToUpper();
		moveDescriptionText.text = combatMove.description;
		int pos = character.data.moves.IndexOf(combatMove) + 1;
		moveWheelIcon.sprite = CombatMove.SpriteForMoveCategory(combatMove.moveCategory);
	}

	public void SpinWheel() {
		Arena arena = GameManager.gm.arena;
		if(arena.rerolls <= 0 )
			return;
		arena.rerolls--;
		arena.combatUI.RefreshRerolls();
		arena.combatUI.HideRerollButtons();
		character.queuedMove = null;
		RevealWheel();
		arena.combatUI.RollWheel(this, UnityEngine.Random.Range(1000f, 2000f));
		arena.state = Arena.State.SingleMoveRollPhase;
	}
}

