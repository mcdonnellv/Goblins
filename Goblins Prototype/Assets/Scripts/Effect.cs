using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Modifier : MonoBehaviour { 
	public float amount = 1f;
	public int criteria;

}

//this is for buffs to attributes only, not status effects
public class Effect : MonoBehaviour {

	// this effect may have many attribute modifiers (like +10 to body and +5 to defense)
	public List<Modifier> modifiers;


	//multiplicative effects only
	public float ApplyAttributeModifier(int attributeID, int criteria, float val) {
		Modifier modifier = modifiers[attributeID];

		if (modifier.criteria == criteria)
			return val * modifier.amount;
		return val;
	}


}

public class OtherCharClass : MonoBehaviour {

	public List<float> rawAttributeValue;
	public List<Effect> allEffects;


	public float GetCurrentAttributeValue(int attributeID, int criteria) {
		float val = rawAttributeValue[attributeID];
		foreach(Effect effect in allEffects)
			val = effect.ApplyAttributeModifier(attributeID, criteria, val);
		return val;
	}
}
