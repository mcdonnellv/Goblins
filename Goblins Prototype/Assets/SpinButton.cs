using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpinButton : MonoBehaviour {

	public void Pressed () {
		GoblinCombatPanel panel = transform.parent.GetComponentInChildren<GoblinCombatPanel>();
		panel.SpinWheel();
	}
}
