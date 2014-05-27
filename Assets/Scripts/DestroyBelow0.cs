using UnityEngine;
using System.Collections;

public class DestroyBelow0 : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position.y < 0) Object.Destroy(this.gameObject);
	}
}
