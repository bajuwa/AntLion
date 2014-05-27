using UnityEngine;
using System.Collections;

public class introScript : MonoBehaviour {

	private const string BUTTON_SOUND_OBJECT_NAME = "ButtonSound";
	public GameObject buttonSound;
	
	public Texture2D contButton;
	
	int buttonWidth = 189;
	int buttonHeight = 90;
	public int bufferBetweenButtonAndScreenEdge = 10;
	
	// Use this for initialization
	void Start () {
		buttonSound = GameObject.Find(BUTTON_SOUND_OBJECT_NAME);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void OnGUI() {
		// Positioning of Continue button
		Rect contRect = new Rect(Screen.width - buttonWidth - bufferBetweenButtonAndScreenEdge, 
								 Screen.height - buttonHeight - bufferBetweenButtonAndScreenEdge, 
								 buttonWidth, buttonHeight);
		GUI.DrawTexture(contRect, contButton);
		Event e = Event.current;
		if (e.type == EventType.MouseUp) {
			if (contRect.Contains (e.mousePosition)) {
				Application.LoadLevel ("Tutorial");
			}
		}
		
		// Play a sound when the player clicks on a button (releasing the button doesn't matter)
		if (e.type == EventType.MouseDown && contRect.Contains(e.mousePosition)) {
			buttonSound.audio.Play();
		}
	}
}
