using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CombatClass {
	public CombatClassType type;
	public List<Transform> movePrefabs = new List<Transform>();
}
