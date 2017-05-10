using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GoblinCombatPanel : MonoBehaviour {
	public int position;
	public Text positionText;
	public Text classText;
	public Text energyText;
	public GameObject curtain;
	public GameObject wheelCover;
	public LifeBar lifeBar;
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
	public GameObject moveDetails;
	public Image moveIcon;
	public Text moveNameText;
	public Text moveDescriptionText;
	public Text moveEnergyText;
	public Text moveDamageText;
	public Text moveNumberText;
	public Image moveNumberBg;
	public GameObject highlight;

	public void Setup(Character c) {
		character = c;
		lifeBar.Setup(c);
		iconImage.sprite = character.data.combatClass.icon;
		classText.text = character.data.combatClass.type.ToString();
		RefreshMoveNames();
		RefreshBars();
		GetComponent<CanvasGroup>().alpha = 1f;
		SetOpponent(null);
		moveDetails.SetActive(false);
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
		energyText.text = character.data.energy.ToString() + "/" + character.data.maxEnergy.ToString();
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
		if(o == null || o.state == Character.State.Dead) {
			opponentInfo.SetActive(false);
			return;
		}
		opponentInfo.SetActive(true);
		opponentImage.sprite = o.spriteRenderer.sprite;
		opponentImage.preserveAspect = true;
		RefreshBar(opponentLifeBar, opponent.data.life, opponent.data.maxLife, 70f);
	}

	public void Pressed() {
		float time = float.MaxValue;
		CombatUI cui = GameManager.gm.arena.combatUI;
		cui.HideEnemyPanel();
		foreach(Transform child in cui.targetPointerContainers)
			Destroy(child.gameObject);

		if(character != null) {
			GameManager.gm.arena.selectedChar = character;
			cui.ShowTargetPointer(character, time);
		}
		if(opponent != null){
			cui.ShowTargetPointer(opponent, time);
			cui.ShowEnemyPanel(opponent);
		}
		foreach(GoblinCombatPanel gcp in cui.goblinPanels)
			gcp.SetInHighlightedStatus();
	}

	public void HideWheel() {
		wheelCover.GetComponent<Animator>().SetBool("revealed", false);
	}
	public void RevealWheel() {
		wheelCover.GetComponent<Animator>().SetBool("revealed", true);
	}

	public void DisplayMove() {
		if(character != null && character.queuedMove != null) {
			CombatMove combatMove = character.queuedMove;
			moveDetails.SetActive(true);

			moveNameText.text = combatMove.moveName;
			moveIcon.sprite = combatMove.sprite;
			moveIcon.color = combatMove.ColorFromDamageType();
			moveDescriptionText.text = combatMove.description;//GenerateDesciption();
			moveEnergyText.text = combatMove.energyCost + " Energy";
			moveDamageText.text = combatMove.damageType.ToString() + " Damage";
			moveDamageText.color = moveIcon.color;
			int pos = character.data.moves.IndexOf(combatMove) + 1;
			moveNumberText.text = pos.ToString();
			moveNumberBg.color = CombatMove.ColorFromMovePosition(pos);
			if(combatMove.damageType == CombatMove.DamageType.None)
				moveDamageText.text = "";
			if(combatMove.moveType == CombatMove.MoveType.Heal) {
				moveDamageText.text = "Healing";
				moveDamageText.color = Color.green;
				moveIcon.color = Color.green;
			}
		}
	}
}

