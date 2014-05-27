using UnityEngine;
using System.Collections;

public class UndyingMusic : MonoBehaviour {

	private static UndyingMusic instance;

	// Use this for initialization
	void Awake () { 
		if (instance && instance != this) Destroy(this.gameObject);
		else instance = this;
		
		DontDestroyOnLoad(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
