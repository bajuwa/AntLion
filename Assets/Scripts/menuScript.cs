using UnityEngine;

/// Title screen script
public class menuScript : MonoBehaviour
{
	public Transform buttonUnpressed;
	public Transform buttonPressed;

	void OnGUI()
	{
		Event e = Event.current;
		if (e.type == EventType.MouseDown) {
				
		}

		if (e.type == EventType.MouseUp) {
			// On Click, load the first intro.
			Application.LoadLevel("Intro");
		}
	}
}