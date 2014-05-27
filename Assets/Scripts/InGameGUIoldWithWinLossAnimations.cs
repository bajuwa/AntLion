using UnityEngine;
using System.Collections;

public class InGameGUIoldWithWinLossAnimations : MonoBehaviour {

	private AntBehaviour antBehaviour;
	private AntLion antLionBehaviour;
	
	private const string LANDSLIDE_MANAGER_OBJECT_NAME = "Landslide Manager";
	private LandslideManager landslideManager;

	// Use this for initialization
	void Start () {
		Debug.Log("Starting InGameGui");
		antBehaviour = GameObject.Find("antbirdview").GetComponent<AntBehaviour>();
		antLionBehaviour = GameObject.Find("antlionbirdview").GetComponent<AntLion>();
		landslideManager = GameObject.Find(LANDSLIDE_MANAGER_OBJECT_NAME).GetComponent<LandslideManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		GlobalState.GameState state = antBehaviour.CheckWinLose();
		switch (state) {
		case GlobalState.GameState.LOST:
			antLionBehaviour.allowedToAttack = false;
			if (!antBehaviour.isFalling()) antBehaviour.triggerLandslideBehaviour();
			if (!antLionBehaviour.isEating()) antLionBehaviour.startEatingAnimation();
			GUI.Box(new Rect(Screen.width/2 - 50, Screen.height/2 - 25, 100, 50), "YOU LOSE");
			if (GUI.Button (new Rect(Screen.width/2 - 40, Screen.height/2 - 5, 80, 20), "Restart")) {
				Application.LoadLevel("Menu");
			}
			break;
		case GlobalState.GameState.WON:
			antLionBehaviour.allowedToAttack = false;
			// If we won, we want the ant lion to be 'defeated' by a landslide
			if (!antLionBehaviour.isFalling()) antLionBehaviour.startFallingAnimation();
			if (!landslideManager.isGridLandsliding()) landslideManager.animateFakeLandslide();
			
			GUI.Box(new Rect(Screen.width/2 - 50, Screen.height/2 - 25, 100, 50), "YOU WIN");
			if (GUI.Button (new Rect(Screen.width/2 - 40, Screen.height/2 - 5, 80, 20), "Restart")) {
				Application.LoadLevel("Menu");
			}
			break;
		}
	}
}
