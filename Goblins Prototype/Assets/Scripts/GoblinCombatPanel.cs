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
	public GameObject curtain;
	public Image lifeBar;
	public Image energyBar;
	public Image iconImage;
	public Image opponentImage;
	public Image opponentLifeBar;
	public GameObject opponentInfo;
	public Character character;
	public Character opponent;
	public InfiniteScroll wheel;
	public List<Text> moveLabels;
	public CharacterDetails characterDetails;

	public void Setup(Character c) {
		character = c;
		iconImage.sprite = character.data.combatClass.icon;
		classText.text = character.data.combatClass.type.ToString();
		RefreshMoveNames();
		RefreshBars();
		GetComponent<CanvasGroup>().alpha = 1f;
		SetOpponent(null);
		curtain.SetActive(false);
	}

	public void RefreshBars() {
		lifeText.text = character.data.life.ToString() + "/" + character.data.maxLife.ToString();
		energyText.text = character.data.energy.ToString() + "/" + character.data.maxEnergy.ToString();
		RefreshBar(lifeBar, character.data.life, character.data.maxLife, 172f);
		RefreshBar(energyBar, character.data.energy, character.data.maxEnergy, 172f);
		if(opponent != null)
			RefreshBar(opponentLifeBar, opponent.data.life, opponent.data.maxLife, 70f);
	}

	void RefreshBar(Image bar, float curval, float totVal, float v) {
		Vector2 s = bar.GetComponent<RectTransform>().sizeDelta;
		bar.GetComponent<RectTransform>().sizeDelta = new Vector2(v * curval/totVal, s.y);
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
		Debug.Log("\tGoblin " + character.data.givenName +  " has rolled: " + character.queuedMove.moveName + "\n");
		GameManager.gm.arena.CheckAllGoblinMovesSelected();
	}

	public void SetOpponent(Character o) {
		opponent = o;
		if(o == null) {
			opponentInfo.SetActive(false);
			return;
		}
		opponentInfo.SetActive(true);
		opponentImage.sprite = o.spriteRenderer.sprite;
		opponentImage.preserveAspect = true;
		RefreshBar(opponentLifeBar, opponent.data.life, opponent.data.maxLife, 70f);
	}

	public void Pressed() {
		characterDetails.AssignCharacter(character.data);
		characterDetails.gameObject.SetActive(true);
	}
}

