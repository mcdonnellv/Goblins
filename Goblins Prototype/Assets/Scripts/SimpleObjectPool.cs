using System.Collections.Generic;
using UnityEngine;

public class SimpleObjectPool : MonoBehaviour {
	public GameObject prefab;
	private Stack<GameObject> inactiveInstances = new Stack<GameObject>();

	public GameObject GetObject() {
		GameObject spawnedGameObject;

		if(inactiveInstances.Count > 0) {
			spawnedGameObject = inactiveInstances.Pop();
		}
		else {
			spawnedGameObject = (GameObject)GameObject.Instantiate(prefab);
			PooledObject pooledObject = spawnedGameObject.AddComponent<PooledObject>();
			pooledObject.pool = this;

		}

		spawnedGameObject.transform.SetParent(null);
		spawnedGameObject.SetActive(true);
		return spawnedGameObject;
	}

	public void ReturnObject(GameObject toReturn) {
		PooledObject pooledObject = toReturn.GetComponent<PooledObject>();

		if(pooledObject != null && pooledObject.pool == this) {
			toReturn.transform.SetParent(transform, false);
			toReturn.SetActive(false);
			inactiveInstances.Push(toReturn);
		}
		else {
			Debug.LogWarning(toReturn.name + "was returned to a pool it wasn't from! Destroying");
			Destroy(toReturn);
		}
	}
}

