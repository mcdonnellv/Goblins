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
		nameLabel.text = combatMove.moveName.ToUpper();
		moveIcon.sprite = CombatMove.SpriteForMoveCategory(combatMove.moveCategory);
		moveIcon.color = CombatMove.ColorForMoveCategory(combatMove.moveCategory);
		descriptionLabel.text = combatMove.description;
	}
}
