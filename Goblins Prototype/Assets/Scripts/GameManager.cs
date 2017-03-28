using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {
	private static GameManager _gm;
	public static GameManager gm { get {if(_gm == null) _gm = GameObject.Find("GameManager").GetComponent<GameManager>(); return _gm; } }

	public Roster roster;
	public Enemies enemies;
	public List<CombatMove> moves = new List<CombatMove>();

	public enum State {
		Init,
		Prep,
		Combat,
		Result,
	}

	public State state;

	IEnumerator InitState () {
		Debug.Log("Init: Enter");
		roster.Populate();
		while (state == State.Init) {
			yield return 0;
		}
		Debug.Log("Init: Exit");
		NextState();
	}

	IEnumerator PrepState () {
		Debug.Log("Prep: Enter");
		while (state == State.Prep) {
			yield return 0;
		}
		Debug.Log("Prep: Exit");
		NextState();
	}

	IEnumerator CombatState () {
		Debug.Log("Combat: Enter");
		while (state == State.Combat) {
			yield return 0;
		}
		Debug.Log("Combat: Exit");
	}

	IEnumerator ResultState () {
		Debug.Log("Result: Enter");
		while (state == State.Result) {
			yield return 0;
		}
		Debug.Log("Result: Exit");
	}



	void NextState () {
		string methodName = state.ToString() + "State";
		System.Reflection.MethodInfo info =
			GetType().GetMethod(methodName,
				System.Reflection.BindingFlags.NonPublic |
				System.Reflection.BindingFlags.Instance);
		StartCoroutine((IEnumerator)info.Invoke(this, null));
	}

	void Start () {
		NextState();

		roster.Populate();
	}

	// Update is called once per frame
	void Update () {

	}

}