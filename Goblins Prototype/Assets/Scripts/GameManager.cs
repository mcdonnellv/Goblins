using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameManager : MonoBehaviour {
	private static GameManager _gm;
	public static GameManager gm { get {if(_gm == null) _gm = GameObject.Find("GameManager").GetComponent<GameManager>(); return _gm; } }

	public Roster roster;
	public Enemies enemies;
	public List<CombatMove> moves = new List<CombatMove>();
	public Camera prepCam;
	public Camera fightCam;
	public Arena arena;
	public GameObject prepUI;

	public enum State {
		Init,
		Prep,
		Combat,
		Result,
	}

	public State state;

	IEnumerator InitState () {
		Debug.Log("Init: Enter\n");
		roster.Populate();
		state = State.Prep;
		arena.combatUI.gameObject.SetActive(false);
		while (state == State.Init) {
			yield return 0;
		}
		Debug.Log("Init: Exit\n");
		NextState();
	}

	IEnumerator PrepState () {
		Debug.Log("Prep: Enter\n");
		prepCam.enabled = true;
		fightCam.enabled = false;

		while (state == State.Prep) {
			yield return 0;
		}
		Debug.Log("Prep: Exit\n");
		NextState();
	}

	IEnumerator CombatState () {
		Debug.Log("Combat: Enter\n");
		roster.gameObject.SetActive(false);
		prepCam.enabled = false;
		fightCam.enabled = true;
		prepUI.gameObject.SetActive(false);
		arena.state = Arena.State.Init;
		while (state == State.Combat) {
			yield return 0;
		}
		arena.state = Arena.State.Inactive;
		Debug.Log("Combat: Exit\n");
		NextState();
	}

	IEnumerator ResultState () {
		Debug.Log("Result: Enter\n");
		arena.combatUI.gameObject.SetActive(false);
		//copy survivors to prep
		prepUI.gameObject.SetActive(true);
		PartyPanel pp = prepUI.gameObject.GetComponentInChildren<PartyPanel>(true);
		pp.gameObject.SetActive(true);
		int i=0;
		foreach(Character goblin in arena.goblins) {
			if(goblin.state != Character.State.Dead)
				pp.partyPanels[i].Setup(goblin.data);
			i++;
		}

		foreach(Character enemy in arena.enemies)
			enemy.DeSpawn();
			
		state = State.Prep;
		while (state == State.Result) {
			yield return 0;
		}
		Debug.Log("Result: Exit\n");
		NextState();
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
	}

	// Update is called once per frame
	void Update () {

	}

	public IEnumerator MoveOverSeconds (GameObject objectToMove, Vector3 end, float seconds) {
		float elapsedTime = 0;
		Vector3 startingPos = objectToMove.transform.position;
		while (elapsedTime < seconds)
		{
			objectToMove.transform.position = Vector3.Lerp(startingPos, end, (elapsedTime / seconds));
			elapsedTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		objectToMove.transform.position = end;
	}

}