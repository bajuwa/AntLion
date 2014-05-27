using UnityEngine;
using System.Collections;

public class CameraManager : MonoBehaviour {

	// Set this up by attaching what our main/normal camera will be
	public Camera mainCamera;
	public Camera mazePreviewCamera;
	
	// the minimum y coord that the camera is allowed to be centered at
	public float minimumYposition = -1.0f;
	
	// What the user needs to press/hold down to activate the secondary preview camera
	public KeyCode activatePreview;
	
	// This will determine the y position of the camera relative to its target
	// Example: 0 will be dead center, -1 will be below the center of the target, 1 is above, etc...
	public float distanceOffsetFromTargetToMainCamera = 0.0f;
	public float timelapseBetweenShakes = 0.02f;

	private const string PLAYER_OBJECT_NAME = "antbirdview";
	private GameObject target;
	private AntBehaviour targetBehaviour;
	private float heightOfTarget;

	private const string LANDSLIDE_MANAGER_OBJECT_NAME = "Landslide Manager";
	private LandslideManager landslideManager;

	// Use this for initialization
	void Start () {
		target = GameObject.Find(PLAYER_OBJECT_NAME);
		targetBehaviour = target.GetComponent<AntBehaviour>();
		heightOfTarget = target.GetComponent<BoxCollider2D>().size.y * target.transform.localScale.y;
		landslideManager = GameObject.Find(LANDSLIDE_MANAGER_OBJECT_NAME).GetComponent<LandslideManager>();
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 cameraPos;
		
		// Make sure the camera will always follow the player
		cameraPos = mainCamera.transform.position;
		cameraPos.y = target.transform.position.y + distanceOffsetFromTargetToMainCamera;
		cameraPos.y = Mathf.Max(minimumYposition, cameraPos.y);
		mainCamera.transform.position = cameraPos;
		
		// Make sure our preview camera is also ready
		cameraPos = mazePreviewCamera.transform.position;
		cameraPos.y = mainCamera.transform.position.y + mazePreviewCamera.orthographicSize - heightOfTarget;
		mazePreviewCamera.transform.position = cameraPos;
		
		// While the player holds down our 'switch view' button, swap our cameras
		if (Input.GetKeyDown(activatePreview) && !landslideManager.isGridLandsliding()) {
			mainCamera.enabled = false;
			mazePreviewCamera.enabled = true;
			targetBehaviour.moveable = false;
		} else if (Input.GetKeyUp(activatePreview)) {
			mainCamera.enabled = true;
			mazePreviewCamera.enabled = false;
			targetBehaviour.moveable = !landslideManager.isGridLandsliding();
		}
	}
	
	public IEnumerator shakeCamera(float secondsToShake, float degreeToShake) {
		Vector3 cameraPos;
		float startTime = Time.time;
		while (Time.time - startTime < secondsToShake) {
			cameraPos = mainCamera.transform.position;
			
			cameraPos.y += degreeToShake;
			cameraPos.x += degreeToShake;
			mainCamera.transform.position = cameraPos;
			yield return new WaitForSeconds(timelapseBetweenShakes);
			
			cameraPos.y -= degreeToShake;
			cameraPos.x -= degreeToShake;
			mainCamera.transform.position = cameraPos;
			yield return new WaitForSeconds(timelapseBetweenShakes);
		}
	}
}
