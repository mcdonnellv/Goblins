using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCombatPanel : MonoBehaviour {
	public int position;
	public Text positionText;
	public Text nameText;
	public LifeBar lifeBar;
	public Text raceText;
	public Text defenseText;
	public Transform resistanceTextPrefab;
	public Transform resistanceGrid;
	public List<Text> resistances;
	public Character character;
	public Image unitTypeBg;
	public Image unitTypeIcon;

	public void Setup(Character c) {
		character = c;
		lifeBar.Setup(c);
		nameText.text = character.data.givenName;
		raceText.text = character.data.race;
		defenseText.text = "Evasion: " + (character.data.defense * 100f).ToString() + "%";
		positionText.text = character.combatPosition.ToString();
		unitTypeIcon.sprite = Character.SpriteForUnitType(c.data.unitType);
		unitTypeBg.color = Character.ColorForUnitType(c.data.unitType);

		while(resistanceGrid.childCount > 0) {
			Transform child = resistanceGrid.GetChild(0);
			child.SetParent(GameManager.gm.arena.combatUI.transform);
			Destroy(child.gameObject);
		}

		if(character.data.sliceRes != 0)
			MakeResitanceText("Slicing", character.data.sliceRes);
		if(character.data.crushRes != 0)
			MakeResitanceText("Crushing", character.data.crushRes);
		if(character.data.aracaneRes != 0)
			MakeResitanceText("Arcane", character.data.aracaneRes);
		if(character.data.darkRes != 0)
			MakeResitanceText("Dark", character.data.darkRes);
		if(character.data.fireRes != 0)
			MakeResitanceText("Fire", character.data.fireRes);
		if(character.data.coldRes != 0)
			MakeResitanceText("Cold", character.data.coldRes);

		if(resistanceGrid.childCount == 0) {
			Text resistanceText = Instantiate(resistanceTextPrefab).GetComponent<Text>();
			resistanceText.transform.SetParent(resistanceGrid, false);
			resistanceText.transform.localScale = new Vector3(1f,1f,1f);
			resistanceText.text = "None";
		}
		
		lifeBar.Refresh();
	}

	public void MakeResitanceText(string resName, float resVal) {
		//string t = (resVal > 0f ? "Resits " : "Weak to ") + resName;
		Text resistanceText = Instantiate(resistanceTextPrefab, resistanceGrid).GetComponent<Text>();
		resistanceText.transform.SetParent(resistanceGrid, false);
		resistanceText.transform.localScale = new Vector3(1f,1f,1f);
		resistanceText.text = resName + " " + (resVal * 100f).ToString() + "%";
		resistanceText.color = resVal > 0f ? Color.white : new Color(1f, .4f, .4f);
	}

	void RefreshBar(Image bar, float curval, float totVal) {
		Vector2 s = bar.GetComponent<RectTransform>().sizeDelta;
		bar.GetComponent<RectTransform>().sizeDelta = new Vector2(192f * curval/totVal, s.y);
	}
}
