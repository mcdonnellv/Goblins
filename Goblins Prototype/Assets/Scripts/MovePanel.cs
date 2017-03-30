using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovePanel : MonoBehaviour {

	public CombatMove combatMove;
	public Text nameLabel;
	public Text descriptionLabel;
	public Text energyCostLabel;

	// Use this for initialization
	void Start () {
		
	}
	
	public void Setup (CombatMove combatMove) {
		nameLabel.text = combatMove.moveName;
		descriptionLabel.text = combatMove.GenerateDesciption();
		energyCostLabel.text = combatMove.energyCost + " Energy";
	}
}
