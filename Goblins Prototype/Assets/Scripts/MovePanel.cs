using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovePanel : MonoBehaviour {

	public CombatMove combatMove;
	public Text nameLabel;
	public Text descriptionLabel;
	public Image moveIcon;

	public void Setup (CombatMove combatMove) {
		nameLabel.text = combatMove.moveName;
		moveIcon.sprite = CombatMove.SpriteForMoveCategory(combatMove.moveCategory);
		descriptionLabel.text = combatMove.description;
	}
}
