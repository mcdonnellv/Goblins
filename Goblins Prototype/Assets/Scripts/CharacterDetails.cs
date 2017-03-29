using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterDetails : MonoBehaviour {

	public Button closeButton;
	public CharacterData character;


	public void CloseButtonPressed() {
		transform.gameObject.SetActive(false);
	}


	// Use this for initialization
	void Start () {
		transform.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
