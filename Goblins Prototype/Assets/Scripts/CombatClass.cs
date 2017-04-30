using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatClass {
	public CombatClassType type;
	public Sprite sprite;
	public Sprite icon;
	public List<Transform> movePrefabs = new List<Transform>();
}
