using UnityEngine;
using System.Collections;

public class AntLion : MonoBehaviour {
	protected Animator animator;
	private const string ATTACK_PARAM = "isAttacking";
	
	private const string MAZE_OBJECT_NAME = "Grid";
	private GridMaker mazeScript;
	
	private const string PLAYER_OBJECT_NAME = "antbirdview";
	private GameObject target;
	
	private const string MAIN_CAMERA_OBJECT_NAME = "Main Camera";
	private GameObject mainCamera;
	
	private AudioSource dyingAudio;
	private AudioSource eatingAudio;
	
	// The percent chance the AntLion will have to attack per second (where 0.05 represents 5%)
	public float chanceToAttackPerSecond = 0.1f;
	// How fast the AntLion will rotate when picking a wall to break (or following the player)
	public float movementSpeed = 1.0f;
	// How long the AntLion will chomp at the wall before it breaks
	public float wallBreakAnimationDuration = 1.5f;
	// The distance from the center of the camera to the antlion
	public float distanceBelowCenterOfCamera = 3.0f;
	// Dictates whether the antlion is allowed to break walls (may need to disable after winning/losing, etc)
	public bool allowedToAttack = true;
	// How fast the antlion will fall when you win
	public float gravityScaleWhileFalling = 5.0f;
	
	private bool eating = false;
	
	private Vector2 coordToLookAt;
	private Quaternion lookRotation;
	
	public void startEatingAnimation() {
		eating = true;
		eatingAudio.Play();
		animator.SetBool(ATTACK_PARAM, true);
	}
	
	public bool isEating() {
		return eating;
	}
	
	public void startFallingAnimation() {
		dyingAudio.Play();
		animator.SetBool(ATTACK_PARAM, true);
		rigidbody2D.gravityScale = gravityScaleWhileFalling;
	}
	
	public bool isFalling() {
		return (rigidbody2D.gravityScale > 0);
	}

	// Use this for initialization
	void Start () {
		Debug.Log("Setting up AntLion animator");
		animator = GetComponent<Animator>();
		animator.SetBool(ATTACK_PARAM, false);
		
		AudioSource[] audios = GetComponents<AudioSource>();
		dyingAudio = audios[0];
		eatingAudio = audios[1];
		
		Debug.Log("Setting up connection to Maze script");
		mazeScript = GameObject.Find(MAZE_OBJECT_NAME).GetComponent<GridMaker>();
		
		Debug.Log("Grabbing target for AntLion to follow");
		target = GameObject.Find(PLAYER_OBJECT_NAME);
		
		Debug.Log("Grabbing camera that determines antlions position");
		mainCamera = GameObject.Find(MAIN_CAMERA_OBJECT_NAME);
		
		Debug.Log("Start calculations for attacks");
		StartCoroutine(makeDecisions());
	}

	// Update is called once per frame
	void Update () { 
		// We only want to manually update the antlion if he isn't doing a freefall (due to winning)
		if (rigidbody2D.gravityScale > 0) return;
	
		// Update the angle of the antlion to point at its target (either a wall or player)
		Vector3 targetPos = target.transform.position;
		// Adjust for the player (and possibly walls) being anchored at the bottom left instead of center
		targetPos.x = targetPos.x + (target.renderer.bounds.size.x/2);
		targetPos.y = targetPos.y + (target.renderer.bounds.size.y/2);
		// Update angle
		transform.up = targetPos - transform.position;
		
		// Make sure antlion stays positioned in relation to the camera
		Vector2 myPos = transform.position;
		myPos.y = mainCamera.transform.position.y - distanceBelowCenterOfCamera;
		transform.position = myPos;
	}

	private IEnumerator makeDecisions() {
		while (true) {
			// Only make a decision every second
			yield return new WaitForSeconds(1);
			
			// AntLion should randomly decide to break walls
			if (allowedToAttack && Random.Range(0.0f,1.0f) < chanceToAttackPerSecond) {
				GameObject wallToDestroy = mazeScript.getWallWithinPlayerRange();
				if (wallToDestroy != null) {
					// Angling AntLion to face the wall it will break
					target = wallToDestroy;
					yield return new WaitForSeconds(1);
					if (!allowedToAttack) {					
						target = GameObject.Find(PLAYER_OBJECT_NAME);
						continue;
					}
					
					// Begin our 'attack'
					animator.SetBool(ATTACK_PARAM, true);
					wallToDestroy.audio.Play();
					wallToDestroy.particleSystem.Play();
					
					// We don't want the attack to be instantaneous
					yield return new WaitForSeconds(wallBreakAnimationDuration);
					
					// Now destroy the object and stop the 'attack'
					animator.SetBool(ATTACK_PARAM, false);
					if (allowedToAttack) {
						mazeScript.breakWall(wallToDestroy);
					} else {
						wallToDestroy.audio.Stop();
						wallToDestroy.particleSystem.Stop();
					}
					
					// reset our target back to the player
					target = GameObject.Find(PLAYER_OBJECT_NAME);
				}
			}
		}
	}
}
