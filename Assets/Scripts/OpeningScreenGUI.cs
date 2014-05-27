using UnityEngine;
using System.Collections;

public class OpeningScreenGUI : MonoBehaviour {

	public Texture2D casual;
	public Texture2D casualPressed;
	public Texture2D normal;
	public Texture2D normalPressed;
	public Texture2D veteran;
	public Texture2D veteranPressed;
	
	int buttonWidth = 150;
	int buttonHeight = 71;

	private GlobalState gameState;
	
	public GameObject buttonSound;

	// Use this for initialization
	void Start () {
		gameState = GameObject.Find("GlobalState").GetComponent<GlobalState>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnGUI () {
		// Positioning of CASUAL button
		Rect casualRect = new Rect(Screen.width / 2 - (buttonWidth / 2), Screen.height - 105, buttonWidth, buttonHeight);
		GUI.DrawTexture(casualRect, casualPressed);
		Event casualEvent = Event.current;
		if (casualRect.Contains (casualEvent.mousePosition)) {
				GUI.DrawTexture (casualRect, casual);
			}
		if (casualEvent.type == EventType.MouseUp) {
			if (casualRect.Contains(casualEvent.mousePosition)) {
				gameState.difficulty = GlobalState.Difficulty.EASY;
				Application.LoadLevel ("Intro");
			}
		}

		// Positioning of NORMAL button
		Rect normalRect = new Rect(Screen.width / 2 - (buttonWidth / 2), Screen.height - 185, buttonWidth, buttonHeight);
		GUI.DrawTexture(normalRect,  normalPressed);
		Event normalEvent = Event.current;
		if (normalRect.Contains (normalEvent.mousePosition)) {
			GUI.DrawTexture (normalRect, normal);
		}
		if (normalEvent.type == EventType.MouseUp) {
			if (normalRect.Contains(normalEvent.mousePosition)) {
				gameState.difficulty = GlobalState.Difficulty.MEDIUM;
				Application.LoadLevel ("Intro");
			}
		}

		// Positioning of VETERAN button
		Rect veteranRect = new Rect(Screen.width / 2 - (buttonWidth / 2), Screen.height - 265, buttonWidth, buttonHeight);
		GUI.DrawTexture(veteranRect,  veteranPressed);
		Event veteranEvent = Event.current;
		if (veteranRect.Contains (veteranEvent.mousePosition)) {
			GUI.DrawTexture (veteranRect, veteran);
		}
		if (veteranEvent.type == EventType.MouseUp) {
			if (veteranRect.Contains(veteranEvent.mousePosition)) {
				gameState.difficulty = GlobalState.Difficulty.HARD;
				Application.LoadLevel ("Intro");
			}
		}
		
		// Play a sound when the player clicks on a button (releasing the button doesn't matter)
		if ((casualEvent.type == EventType.MouseDown && casualRect.Contains(veteranEvent.mousePosition)) || 
			(normalEvent.type == EventType.MouseDown && normalRect.Contains(veteranEvent.mousePosition)) || 
			(veteranEvent.type == EventType.MouseDown && veteranRect.Contains(veteranEvent.mousePosition))) {
			buttonSound.audio.Play();
		}
	}
}
