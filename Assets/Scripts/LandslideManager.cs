using UnityEngine;
using System.Collections;

public class LandslideManager : MonoBehaviour {
	
	// The chance that breaking a wall will cause a landslide
	public GameObject[] landslideRocks;
	public float chanceToCauseLandslide = 0.3f;
	public float landslideDurationSeconds = 2.0f;
	public float rocksPerSecond = 15.0f;
	public float minimumGravityScaleOfRocks = 0.1f;
	public float maximumGravityScaleOfRocks = 1.5f;
	public float degreesToShakeCameraInLandslide = 0.02f;
	
	private bool isLandsliding = false;
	
	private const string PLAYER_OBJECT_NAME = "antbirdview";
	private AntBehaviour playerScript;
	
	private const string ANTLION_OBJECT_NAME = "antlionbirdview";
	private AntLion antlionScript;
	
	private const string GRID_MAKER_OBJECT_NAME = "Grid";
	private GridMaker gridMaker;
	
	private const string CAMERA_MANAGER_OBJECT_NAME = "Camera Manager";
	private CameraManager cameraManager;

	// Use this for initialization
	void Start () {
		playerScript = GameObject.Find(PLAYER_OBJECT_NAME).GetComponent<AntBehaviour>();
		antlionScript = GameObject.Find(ANTLION_OBJECT_NAME).GetComponent<AntLion>();
		gridMaker = GameObject.Find(GRID_MAKER_OBJECT_NAME).GetComponent<GridMaker>();
		cameraManager = GameObject.Find(CAMERA_MANAGER_OBJECT_NAME).GetComponent<CameraManager>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public void calculateLandslide() {
		if (!isLandsliding && Random.Range(0.0f, 1.0f) < chanceToCauseLandslide) {
			StartCoroutine(landslide(true));
		}
	}
	
	public void animateFakeLandslide() {
		StartCoroutine(landslide(false));
	}
	
	public bool isGridLandsliding() {
		return isLandsliding;
	}
	
	private IEnumerator landslide(bool affectPlayer) {
		// Disable actions during a landslide
		isLandsliding = true;
		antlionScript.allowedToAttack = false;
		if (affectPlayer) {
			playerScript.triggerLandslideBehaviour();
			StartCoroutine(cameraManager.shakeCamera(landslideDurationSeconds, degreesToShakeCameraInLandslide));
		}
		
		// Play the actual landslide by creating landslide rocks and letting them fall from the top of the maze
		StartCoroutine(animateFallingRocks(landslideDurationSeconds));
		yield return new WaitForSeconds(landslideDurationSeconds);
		
		// Reenable actions once complete
		isLandsliding = false;
		antlionScript.allowedToAttack = true;
		if (affectPlayer) playerScript.endLandslideBehaviour();
	}
	
	private IEnumerator animateFallingRocks(float duration) {
		audio.Play();
		// Play the actual landslide by creating landslide rocks and letting them fall from the top of the maze
		GameObject rocks;
		float startTime = Time.time;
		while (Time.time - startTime < duration) {
			// For the duration of the landslide, create rock images at the top of the maze (random distribution over width of maze)
			rocks = (GameObject) Instantiate(landslideRocks[Random.Range(0, landslideRocks.Length)], 
								new Vector2(Random.Range(-gridMaker.getTotalWidth()/2, gridMaker.getTotalWidth()/2), gridMaker.getTotalHeight()),
								Quaternion.identity);
			// Give them random weights so they fall at different speeds
			rocks.rigidbody2D.gravityScale = Random.Range(minimumGravityScaleOfRocks, maximumGravityScaleOfRocks);
			// We only want to make rocks at a certain rate
			yield return new WaitForSeconds(1.0f / rocksPerSecond);
		}
		audio.Stop();
	}
}
