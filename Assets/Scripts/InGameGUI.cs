using UnityEngine;
using System.Collections;

public class InGameGUI : MonoBehaviour {
	public Font timerFont;
	public GUIStyle style;
	private float startTime;
	private float guiTime;
	public string textTime; //added this member variable here so we can access it through other scripts

	public Texture2D time;
	public Texture2D win;
	public Texture2D lose;
	public Texture2D quit;
	public Texture2D replay;
	private Rect screenRect;
	private Rect restart;
	private Rect popupQuit;
	private Rect popupTimeRect;
	private Rect gameQuit;
	
	public float popupTakesUpPercentageOfScreen = 0.7f;
	public float originalPopupWidth = 800;
	public float originalPopupHeight = 600;
	float timerBarWidth = 180;
	float timerBarHeight = 92;
	float originalButtonWidth = 189;
	float originalButtonHeight = 80;
	
	public float pixelDistanceFromGuiToScreenEdge = 10;
	public float timerOffsetX = 90;
	public float timerOffsetY = 35;

	private const string BUTTON_SOUND_OBJECT_NAME = "ButtonSound";
	public GameObject buttonSound;
	
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
		buttonSound = GameObject.Find(BUTTON_SOUND_OBJECT_NAME);
		startTime = Time.time;
		
		float popupWidth = originalPopupWidth * popupTakesUpPercentageOfScreen;
		float popupHeight = originalPopupHeight * popupTakesUpPercentageOfScreen;
		float buttonWidth = originalButtonWidth * popupTakesUpPercentageOfScreen;
		float buttonHeight = originalButtonHeight * popupTakesUpPercentageOfScreen;
		float scale = popupWidth / Screen.width;
		Debug.Log("scale: " + scale);
		screenRect = new Rect(Screen.width / 2 - (popupWidth / 2), 
							  Screen.height / 2 - (popupHeight / 2), 
							  popupWidth, 
							  popupHeight);
		restart = new Rect(screenRect.x + (screenRect.width / 5) - (buttonWidth / 4), 
						   screenRect.y + (5 * screenRect.height / 8), 
						   buttonWidth, 
						   buttonHeight);
		popupQuit = new Rect(screenRect.x + (screenRect.width / 5) - (buttonWidth / 4), 
							 screenRect.y + (3 * screenRect.height / 4), 
							 buttonWidth, 
							 buttonHeight);
		popupTimeRect = new Rect(screenRect.x + (3 * screenRect.width / 11), 
								 screenRect.y + (21 * screenRect.height / 40), 
								 90, 
								 100);
		gameQuit = new Rect(Screen.width - buttonWidth - pixelDistanceFromGuiToScreenEdge, Screen.height - buttonHeight - pixelDistanceFromGuiToScreenEdge, buttonWidth, buttonHeight);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnGUI() {
		Event e = Event.current;

		// Get time
		if (antBehaviour.CheckWinLose() == GlobalState.GameState.PLAYING) {
			guiTime = Time.time - startTime;

			// Convert to mins + secs
			float minutes = guiTime / 60;
			float seconds = guiTime % 60;
			textTime = string.Format ("{0:00}:{1:00}", minutes, seconds);
		
			// Draw timer bar
			Rect timerBar = new Rect(pixelDistanceFromGuiToScreenEdge, Screen.height - timerBarHeight - pixelDistanceFromGuiToScreenEdge, timerBarWidth, timerBarHeight);
			GUI.DrawTexture(timerBar, time);

			// Position timer in timer bar
			GUI.skin.font = timerFont;
			GUI.Label ( new Rect(pixelDistanceFromGuiToScreenEdge + timerOffsetX, Screen.height - timerBarHeight - pixelDistanceFromGuiToScreenEdge + timerOffsetY, 100, 100), textTime, style);

			// In-game Quit Button
			GUI.DrawTexture(gameQuit, quit);
			if (e.type == EventType.MouseUp) {
				if (gameQuit.Contains(e.mousePosition)) {
					Application.LoadLevel ("Menu");
				}
			}
		}

		// Check for win/loss condition
		GlobalState.GameState state = antBehaviour.CheckWinLose();
		switch (state) {
		case GlobalState.GameState.LOST:
			// If we lost, we will be 'eaten' by the antlion
			antLionBehaviour.allowedToAttack = false;
			if (!antBehaviour.isFalling()) antBehaviour.triggerLandslideBehaviour();
			if (!antLionBehaviour.isEating()) antLionBehaviour.startEatingAnimation();
			drawPopupWithTexture(lose);
			break;
		case GlobalState.GameState.WON:
			// If we won, we want the ant lion to be 'defeated' by a landslide
			antLionBehaviour.allowedToAttack = false;
			if (!antLionBehaviour.isFalling()) antLionBehaviour.startFallingAnimation();
			if (!landslideManager.isGridLandsliding()) landslideManager.animateFakeLandslide();
			drawPopupWithTexture(win);
			break;
		}
		
		// Play a sound when the player clicks on a button (releasing the button doesn't matter)
		if (e.type == EventType.MouseDown && 
			(popupQuit.Contains(e.mousePosition) || 
			 restart.Contains(e.mousePosition) ||
			 gameQuit.Contains(e.mousePosition) 
			)) {
			buttonSound.audio.Play();
		}
	}
	
	private void drawPopupWithTexture(Texture2D texture) {
		Event e = Event.current;
		
		// Popup screen
		GUI.DrawTexture(screenRect, texture);
		
		// Restart Button
		GUI.DrawTexture(restart, replay);
		if (e.type == EventType.MouseUp) {
			if (restart.Contains(e.mousePosition)) {
				Application.LoadLevel ("Game");
			}
		}
		
		// Quit Button
		GUI.DrawTexture(popupQuit, quit);
		if (e.type == EventType.MouseUp) {
			if (popupQuit.Contains(e.mousePosition)) {
				Application.LoadLevel ("Menu");
			}
		}

		// Post Time Score
		GUI.Label (popupTimeRect, textTime, style);
	}
}
