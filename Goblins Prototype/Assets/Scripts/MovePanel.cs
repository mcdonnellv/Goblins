using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovePanel : MonoBehaviour {

	public CombatMove combatMove;
	public Text nameLabel;
	public Text descriptionLabel;
	public Text energyCostLabel;
	public Text moveDamageText;
	public Image moveIcon;

	// Use this for initialization
	void Start () {
		
	}
	
	public void Setup (CombatMove combatMove) {
		nameLabel.text = combatMove.moveName;
		moveIcon.sprite = combatMove.sprite;
		moveIcon.color = combatMove.ColorFromDamageType();
		descriptionLabel.text = combatMove.description;//GenerateDesciption();
		energyCostLabel.text = combatMove.energyCost + " Energy";
		moveDamageText.text = "";//combatMove.damageType.ToString() + " Damage";
		moveDamageText.color = moveIcon.color;
		if(combatMove.damageType == CombatMove.DamageType.None)
			moveDamageText.text = "";
		if(combatMove.moveType == CombatMove.MoveType.Heal) {
			//moveDamageText.text = "Healing";
			//moveDamageText.color = Color.green;
			moveIcon.color = Color.green;
		}
	}
}
