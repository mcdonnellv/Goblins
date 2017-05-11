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
	public GameObject winScreen;
	public GameObject loseScreen;
	public GameObject introPage;
	public bool useTimeScale = false;
	public bool autoplay = false;
	public bool showIntroPage = true;
	public float timeScaleMultiplier;

	public enum State {
		Init,
		Prep,
		Combat,
		Result,
		GameEnd,
	}

	public State state;

	IEnumerator InitState () {
		Debug.Log("Init: Enter\n");
		roster.Populate();
		arena.combatUI.gameObject.SetActive(false);
		enemies.Setup();
		if(showIntroPage == false) {
			introPage.SetActive(false);
			prepUI.SetActive(true);
			state = State.Prep;
		}
		else {
			introPage.SetActive(true);
			prepUI.SetActive(false);
		}

		while (state == State.Init) {
			yield return 0;
		}
		Debug.Log("Init: Exit\n");
		NextState();
	}

	IEnumerator PrepState () {
		Debug.Log("Prep: Enter\n");
		prepUI.SetActive(true);
		prepCam.enabled = true;
		fightCam.enabled = false;
		//spawn enemies
		enemies.SetAndSpawnParty(enemies.curPartyIndex);
		enemies.difficutlyModifier += .5f;
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
		foreach(Transform t in arena.playerSpawnSpots) {
			Character goblin = arena.GetTransformCharacter(t, true, false);
			if(goblin == null)
				continue;
			bool removeFromRoster = false;
			if(goblin.state != Character.State.Dead)
				goblin.RecoverFull();
			else 
				foreach(CharacterData d in roster.goblins)
					if(d == goblin.data)
					 removeFromRoster = true;	

			if(removeFromRoster)
				roster.goblins.Remove(goblin.data);
		}

		foreach(Character goblin in arena.goblins)
			goblin.DeSpawn();
		
		foreach(Character enemy in arena.enemies)
			enemy.DeSpawn();

		if(roster.goblins.Count == 0) {
			loseScreen.SetActive(true);
			Debug.Log("Game Over You Lose!\n");
			state = State.GameEnd;
			NextState();
			yield break;
		}
			
		enemies.curPartyIndex++;
		if(enemies.curPartyIndex < enemies.stageEnemySets.Length)
			state = State.Prep;
		else {
			winScreen.SetActive(true);
			Debug.Log("Game Over You Win!\n");
			state = State.GameEnd;
		}

		while (state == State.Result) {
			yield return 0;
		}
		Debug.Log("Result: Exit\n");
		NextState();
	}

	IEnumerator GameEndState () {
		while (state == State.GameEnd)
			yield return 0;
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
		Time.timeScale = useTimeScale ? timeScaleMultiplier : 1f;
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

	public void PlayPressed() {
		introPage.SetActive(false);
		state = State.Prep;
	}

}