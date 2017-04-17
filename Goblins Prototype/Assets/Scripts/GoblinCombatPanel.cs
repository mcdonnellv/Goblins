using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoblinCombatPanel : MonoBehaviour {
	public int position;
	public Text positionText;
	public Text classText;
	public Text lifeText;
	public Text energyText;
	public Image lifeBar;
	public Image energyBar;
	public Character character;
	public InfiniteScroll wheel;
	public List<Text> moveLabels;

	public void Setup(Character c) {
		character = c;
		classText.text = character.data.combatClass.type.ToString();
		RefreshMoveNames();
		RefreshBars();
	}

	public void RefreshBars() {
		lifeText.text = character.data.life.ToString() + "/" + character.data.maxLife.ToString();
		energyText.text = character.data.energy.ToString() + "/" + character.data.maxEnergy.ToString();
		RefreshBar(lifeBar, character.data.life, character.data.maxLife);
		RefreshBar(energyBar, character.data.energy, character.data.maxEnergy);
	}

	void RefreshBar(Image bar, float curval, float totVal) {
		Vector2 s = bar.GetComponent<RectTransform>().sizeDelta;
		bar.GetComponent<RectTransform>().sizeDelta = new Vector2(172f * curval/totVal, s.y);
	}

	void RefreshMoveNames() {
		int i=0;
		foreach(Text label in moveLabels) {
			if( i >= character.data.moves.Count)
				label.text = "-";
			else
				label.text = character.data.moves[i].moveName;
			i++;
		}
	}

	public void SetSelectedMove(int index) {
		character.queuedMove = character.data.moves[index];
		Debug.Log("Goblin " + character.combatPosition.ToString() + " has rolled: " + character.queuedMove.moveName + "\n");
		GameManager.gm.arena.CheckAllGoblinMovesDone();
	}
}

