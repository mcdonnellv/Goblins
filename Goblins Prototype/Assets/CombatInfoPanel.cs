using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class CombatInfoPanel : MonoBehaviour {
	private Character character;
	private Animator animator;
	private CanvasGroup cg;
	public Text charName;
	public Image sigil;
	public Image unitType;
	public Image unitTypeBG;
	public LifeBar lifebar;
	public Text defense;
	public CombatInfoPanel opposingInfoPanel;
	public CombatAdvantage typeAdvantage;
	public CombatAdvantage sigilAdvantage;

	public void Setup(Character c) {
		character = c;
		charName.text = c.name;
		sigil.sprite = Character.SpriteForSigil(c.data.sigil);
		unitType.sprite = Character.SpriteForUnitType(c.data.unitType);
		unitTypeBG.color = Character.ColorForUnitType(c.data.unitType);
		lifebar.Setup(character);
		lifebar.showText = true;
		defense.text = c.data.defense.ToString();
		lifebar.Refresh();
	}
		
	public void Show() {
		CombatUI ui = GameManager.gm.arena.combatUI;
		if(cg == null)
			cg = GetComponent<CanvasGroup>();
		cg.alpha = 1f;
		ui.ShowTargetPointer(character, float.MaxValue);
		ShowAdvantages();
		lifebar.Refresh();
	}

	public void Hide() {
		if(cg == null)
			cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		HideAdvantages();
	}

	private void HideAdvantages() {
		typeAdvantage.Hide();
		sigilAdvantage.Hide();
	}

	private void ShowAdvantages() {
		HideAdvantages();
		if(character.isPlayerCharacter == false)
			return;
		
		Character opponent = opposingInfoPanel.character;
		if(opponent == null)
			return;

		typeAdvantage.iconBG.color = Character.ColorForUnitType(opponent.data.unitType);
		typeAdvantage.icon.sprite = Character.SpriteForUnitType(opponent.data.unitType);
		sigilAdvantage.icon.sprite = Character.SpriteForSigil(opponent.data.sigil);

		CombatMath cm = GameManager.gm.arena.cm;

		//unit type
		float advantage = cm.Advantage(character.data.unitType, opponent.data.unitType);
		if(advantage > 0f) {
			typeAdvantage.text.text = "+" + Mathf.Abs(advantage * 100f).ToString() + "% vs ";
			typeAdvantage.text.color = character.isPlayerCharacter ? Color.green : Color.red;
			typeAdvantage.Show();

		}
		else if(advantage < 0f) {
			typeAdvantage.text.text = "-" + Mathf.Abs(advantage * 100f).ToString() + "% vs ";
			typeAdvantage.text.color = character.isPlayerCharacter ? Color.red : Color.green;
			typeAdvantage.Show();
		}

		//sigil
		advantage = cm.Advantage(character.data.sigil, opponent.data.sigil);
		if(advantage > 0f) {
			sigilAdvantage.text.text = "+" + Mathf.Abs(advantage * 100f).ToString() + "% vs ";
			sigilAdvantage.text.color = character.isPlayerCharacter ? Color.green : Color.red;
			sigilAdvantage.Show();

		}
		else if(advantage < 0f) {
			sigilAdvantage.text.text = "-" + Mathf.Abs(advantage * 100f).ToString() + "% vs ";
			sigilAdvantage.text.color = character.isPlayerCharacter ? Color.red : Color.green;
			sigilAdvantage.Show();
		}
	}


}
