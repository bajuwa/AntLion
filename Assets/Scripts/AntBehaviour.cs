using UnityEngine;
using System.Collections;

public class AntBehaviour : MonoBehaviour {

	private GameObject grid;
	private GridMaker gridMaker;
	private GlobalState globalState;
	private GameObject cameraObject;
	private GameObject sand;
	private Vector2 antSize;
	private Animator animator;
	
	private const string ANIMATOR_PARAM_NAME_STATE = "state";
	public enum movementStates {
		RESTING = 0, 
		WALKING = 1, 
		FALLING = 2
	};
	
	// How fast the sprite will move
	public bool moveable = true;
	public float moveSpeed = 1.0f;
	public float gravityScaleWhenFalling = 10.0f;
	private float verticalMoveSpeed = 0f;
	private float horizontalMoveSpeed = 0f;
	
	// How much space should our player take up inside a maze cell
	public float playerFillsPercentageOfCell = 0.66f;
	
	public bool isFalling() {
		return (rigidbody2D.gravityScale > 0);
	}

	// Use this for initialization
	void Start () {
		Debug.Log("Starting up the ant player!");
		
		//grab the grid maker
		grid = GameObject.Find ("Grid");
		DebugUtils.Assert(grid);
		gridMaker = grid.GetComponent<GridMaker> ();
		DebugUtils.Assert(gridMaker);
		sand = GameObject.Find ("SandBackground");
		DebugUtils.Assert (sand);
		cameraObject = GameObject.Find ("Main Camera");
		DebugUtils.Assert(cameraObject);
		animator = GetComponent<Animator>();


		//make sure we have a global state - create one if it's missing (probably started scene from within Unity editor)
		GameObject glob = GameObject.Find ("GlobalState");
		if (glob) {
			globalState = glob.GetComponent<GlobalState>();
		} else {
			globalState = grid.AddComponent<GlobalState>();
			Debug.LogWarning("Creating global state object on the fly - assuming game is running in editor");
		}

		//size the ant to fill one grid cell, and put it in a bottom centre cell
		BoxCollider2D collider = GetComponent<BoxCollider2D>();
		antSize = collider.size;
		
		// Resize the ant to fit inside a cell, making sure not to distort image
		float scaleFactor = gridMaker.gridCellHeight/antSize.y;
		scaleFactor *= playerFillsPercentageOfCell;
		transform.localScale = new Vector3 (scaleFactor, scaleFactor, 1.0f);
		
		// Reassign the position of the x/y coords to place ant in bottom center cell
		Vector3 antPos = transform.position;
		antPos.x = grid.transform.position.x + (gridMaker.gridSizeHorizontal * gridMaker.gridCellWidth / 2.0f) + (gridMaker.gridCellWidth/2.0f);
		antPos.y = grid.transform.position.y + (gridMaker.gridCellHeight / 2.0f);
		transform.position = antPos;

		UpdateMovement();
	}

	//update the ant location, and handle the current movement
	void UpdateMovement() {
		if (animator.GetInteger(ANIMATOR_PARAM_NAME_STATE) == (int)movementStates.FALLING) return;
		
		if (horizontalMoveSpeed != 0 || verticalMoveSpeed != 0) {
			// First update the actual position
			Vector2 myPos = transform.position;
			myPos.x += horizontalMoveSpeed * Time.deltaTime;
			myPos.y += verticalMoveSpeed * Time.deltaTime;
			transform.position = myPos;
			
			// Now trigger the animation
			animator.SetInteger(ANIMATOR_PARAM_NAME_STATE, (int)movementStates.WALKING);
			if (!audio.isPlaying) audio.Play();
		} else {
			animator.SetInteger(ANIMATOR_PARAM_NAME_STATE, (int)movementStates.RESTING);
			if (audio.isPlaying) audio.Stop();
		}
	}

	// Update is called once per frame
	void Update () {
		// set default speeds in case no directions are chosen
		verticalMoveSpeed = 0;
		horizontalMoveSpeed = 0;
		//if not currently moving, allow new movement based on user input
		if (moveable) {
			if (Input.GetKey (KeyCode.UpArrow)) {
				verticalMoveSpeed = moveSpeed;
				transform.eulerAngles = new Vector3(0, 0, 0);
			} else if (Input.GetKey (KeyCode.DownArrow)) {
				verticalMoveSpeed = -1 * moveSpeed;
				transform.eulerAngles = new Vector3(0, 0, 180);
			} else if (Input.GetKey (KeyCode.LeftArrow)) {
				horizontalMoveSpeed = -1 * moveSpeed;
				transform.eulerAngles = new Vector3(0, 0, 90);
			} else if (Input.GetKey (KeyCode.RightArrow)) {
				horizontalMoveSpeed = moveSpeed;
				transform.eulerAngles = new Vector3(0, 0, -90);
			}
		}
		//always update the ant movement
		UpdateMovement();
	}

	//calculate the current state of the game
	public GlobalState.GameState CheckWinLose() {
		if (transform.position.y >= (grid.transform.position.y + gridMaker.getTotalHeight()))
		{
			// In either win/lose case, we want to prevent player movement input
			moveable = false;
			return GlobalState.GameState.WON;
		} else if (transform.position.y < grid.transform.position.y) {
			// In either win/lose case, we want to prevent player movement input
			moveable = false;
			return GlobalState.GameState.LOST;
		}
		return GlobalState.GameState.PLAYING;
	}
	
	public void triggerLandslideBehaviour() {
		moveable = false;
		audio.Stop();
		rigidbody2D.gravityScale = gravityScaleWhenFalling;
		transform.rotation = Quaternion.identity;
		animator.SetInteger(ANIMATOR_PARAM_NAME_STATE, (int)movementStates.FALLING);
	}
	
	public void endLandslideBehaviour() {
		moveable = true;
		rigidbody2D.gravityScale = 0.0f;
		animator.SetInteger(ANIMATOR_PARAM_NAME_STATE, (int)movementStates.RESTING);
	}
}
