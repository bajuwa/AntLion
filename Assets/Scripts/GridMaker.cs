using UnityEngine;
using System.Collections;

public class GridMaker : MonoBehaviour {

	private int gridSizeVertical;
	public int gridSizeHorizontal = 5;
	public float gridCellWidth = 64.0f;
	public float gridCellHeight = 64.0f;
	
	// Default difficulty level if none is specified
	private GameObject globalState;
	public int selectedDifficulty = 1;
	
	// These two values will help determine the height of the maze, depending on which difficulty is selected
	// Where difficulty levels = 1, 2, 3, 4, ..... , n
	public int baseNumOfMazeRows = 3;  // Each maze will start out at this height
	public int numOfMazeRowsPerDifficultyLevel = 5;  // For each difficulty level, this many rows are added

    // This is for the prefab called 'Wall' (which includes colliders), make sure this is connected via your public variable settings for the Grid
	public GameObject wall;
	
	// This will determine how many walls are broken at the very bottom of the maze
	// This is a MAXIMUM, not a hard value
	public int maxNumOfHolesAtBottomOfMaze = 3;
	
	// When breaking a wall during the game, limit which walls can be broken to within X pixels of the player's current coord
	public float radiusAroundPlayerToBreakWalls = 1.5f;
	
	// Maze creation grids
	private GameObject[,] topBottomWalls;
	private GameObject[,] leftRightWalls;
	private bool[,] visited;
	
	private const string PLAYER_OBJECT_NAME = "antbirdview";
	private GameObject player;
	private AntBehaviour playerScript;
	
	private const string ANTLION_OBJECT_NAME = "antlionbirdview";
	private AntLion antlionScript;
	
	private const string LANDSLIDE_MANAGER_OBJECT_NAME = "Landslide Manager";
	private LandslideManager landslideManager;
	
	private const int GRID_WALL_LAYER = 8;
	
	public float getTotalHeight() {
		return gridSizeVertical * gridCellHeight;
	}
	
	public float getTotalWidth() {
		return gridSizeHorizontal * gridCellWidth;
	}
	
	// Allow other objects break walls during runtime
	public GameObject getWallWithinPlayerRange() {
		//Debug.Log("Breaking a wall during runtime!");
		
		// Get the position of the player (accounting for bottom-left anchor point)
		Vector3 playerPos = player.transform.position;
		playerPos.x += gridCellWidth/2;
		playerPos.y += gridCellHeight/2;
		// Get all the walls that fall within the range of the player
		Collider2D[] wallsWithinRange = Physics2D.OverlapCircleAll(playerPos, radiusAroundPlayerToBreakWalls, 1 << GRID_WALL_LAYER);
		
		// Break a random wall from within that range
		if (wallsWithinRange.Length == 0) {
			Debug.Log("No walls within range to destroy");
			return null;
		}
		return wallsWithinRange[Random.Range(0,wallsWithinRange.Length)].gameObject;
	}
	
	// Causes a landslide
	public void breakWall(GameObject wall) {
		Object.Destroy(wall);
		landslideManager.calculateLandslide();
	}
	
	void Start () {
		player = GameObject.Find(PLAYER_OBJECT_NAME);
		playerScript = player.GetComponent<AntBehaviour>();
		antlionScript = GameObject.Find(ANTLION_OBJECT_NAME).GetComponent<AntLion>();
		landslideManager = GameObject.Find(LANDSLIDE_MANAGER_OBJECT_NAME).GetComponent<LandslideManager>();
		
		// Determine maze height given the selected difficulty
		globalState = GameObject.Find("GlobalState");
		if (globalState) selectedDifficulty = (int) globalState.GetComponent<GlobalState>().difficulty;
		Debug.Log("Difficulty after loading game scene: " + selectedDifficulty);
		gridSizeVertical = baseNumOfMazeRows + (selectedDifficulty * numOfMazeRowsPerDifficultyLevel);
	
		// Keep track of the walls we create (we will need to destroy some in order to create a guaranteed solvable maze)
		topBottomWalls = new GameObject[gridSizeVertical+1, gridSizeHorizontal+1];
		leftRightWalls = new GameObject[gridSizeVertical+1, gridSizeHorizontal+1];
		visited = new bool[gridSizeVertical, gridSizeHorizontal];
	
		// Create the initial grid
        Debug.Log("Creating initial grid");
		GameObject inst = null;
		for (int x = 0; x < gridSizeHorizontal; x++) {
			int y;
			for (y = 0; y < gridSizeVertical; y++) {
				// left/right vertical walls
				inst = (GameObject)Instantiate (wall, transform.position + new Vector3(x*gridCellWidth,y*gridCellHeight+(gridCellHeight/2),-0.1f), Quaternion.identity);
				inst.transform.localScale = new Vector3(0.5f, gridCellHeight/2, 1.0f);
				// if its an edge wall, put it on a separate layer
				if (x != 0) inst.layer = GRID_WALL_LAYER;
				leftRightWalls[y,x] = inst;
				
				// top/bottom horizontal walls
				inst = (GameObject)Instantiate (wall, transform.position + new Vector3(x*gridCellWidth+(gridCellWidth/2),y*gridCellHeight,-0.1f), Quaternion.FromToRotation(transform.up,transform.right));
				inst.transform.localScale = new Vector3(0.5f, gridCellWidth/2, 1.0f);
				inst.layer = GRID_WALL_LAYER;
				topBottomWalls[y,x] = inst;
				
				//if at the end of a row, also place the right side
				if (x == gridSizeHorizontal - 1) {
					inst = (GameObject)Instantiate (wall, transform.position + new Vector3(gridSizeHorizontal*gridCellWidth,y*gridCellHeight+(gridCellHeight/2),-0.1f), Quaternion.identity);
					inst.transform.localScale = new Vector3(0.5f, gridCellHeight/2, 1.0f);
					leftRightWalls[y,x+1] = inst;
				}
			}
			//if at the end of a column, also place the top
			inst = (GameObject)Instantiate(wall, transform.position + new Vector3(x*gridCellWidth+(gridCellWidth/2), gridSizeVertical*gridCellHeight,-0.1f), Quaternion.FromToRotation(transform.up,transform.right));
			inst.transform.localScale = new Vector3(0.5f, gridCellWidth/2, 1.0f);
			topBottomWalls[y,x] = inst;
		}
		
		// Methodically destroy some walls via DFS path finding in order to create a guaranteed solvable path
		// Start at the 'exit' of the maze and create paths recursively
		Debug.Log("Destroying walls to create the maze");
		int[] currentLocation = new int[2]{gridSizeVertical-1, gridSizeHorizontal/2}; // directions: left(0)/right(1), bottom(0)/top(1)
		dfsVisit(currentLocation);

        // Don't forget to create some openings at the bottom, as well as an exit at the top
		Debug.Log("Creating exit at top of maze");
        Object.Destroy(topBottomWalls[gridSizeVertical, Random.Range(0,gridSizeHorizontal)]);
		Debug.Log("Creating random holes in bottom of maze");
		for (int i = 0; i < maxNumOfHolesAtBottomOfMaze; i++) {
			Object.Destroy(topBottomWalls[0, Random.Range(0,gridSizeHorizontal+1)]);
		}
		
		// We no longer need the 'visited' information, so we can free that up
		visited = null;
	}
	
	private void dfsVisit(int[] currentLocation) {
		// First off, mark our current location as being 'visited'
		//Debug.Log("Visiting location <" + arrayToString(currentLocation) + ">");
		visited[currentLocation[0], currentLocation[1]] = true;
		
		// Next, randomly pick a direction to move in
		int[] possibleDirections = new int[4]{0,1,2,3};
		shuffleElements(possibleDirections);
		//Debug.Log("Checking directions in order: <" + arrayToString(possibleDirections) + ">");
		for (int i = 0; i < possibleDirections.Length; i++) {
			// If the direction we chose is both valid (ie not off end of map) and unvisited, destroy the wall in that direction
			//Debug.Log("Checking direction: <" + possibleDirections[i].ToString() + "> from location <" + arrayToString(currentLocation) + ">");
			int[] nextLocation = moveInDirectionIfPossible(currentLocation, possibleDirections[i]);
			
			// Since the way is now clear, visit the next cell and repeat process
			if (nextLocation[0] != currentLocation[0] || nextLocation[1] != currentLocation[1]) {
				dfsVisit(nextLocation);
			}
		}
	}
	
	private int[] moveInDirectionIfPossible(int[] currentLocation, int direction) {
		int[] nextLocation = new int[2];
		nextLocation[0] = currentLocation[0];
		nextLocation[1] = currentLocation[1];
		switch (direction) {
			case 0:
				if (nextLocation[0]+1 > gridSizeVertical-1 || visited[nextLocation[0]+1, nextLocation[1]]) break;
				//Debug.Log("Going up");
				nextLocation[0]++; 
				Object.Destroy(topBottomWalls[nextLocation[0], nextLocation[1]]);
				break;
			case 1:
				if (nextLocation[0]-1 < 0 || visited[nextLocation[0]-1, nextLocation[1]]) break;
				//Debug.Log("Going down");
				Object.Destroy(topBottomWalls[nextLocation[0], nextLocation[1]]);
				nextLocation[0]--;
				break;
			case 2:
				if (nextLocation[1]-1 < 0 || visited[nextLocation[0], nextLocation[1]-1]) break;
				//Debug.Log("Going left");
				Object.Destroy(leftRightWalls[nextLocation[0], nextLocation[1]]);
				nextLocation[1]--;
				break;
			case 3:
				if (nextLocation[1]+1 > gridSizeHorizontal-1 || visited[nextLocation[0], nextLocation[1]+1]) break;
				//Debug.Log("Going right");
				nextLocation[1]++; 
				Object.Destroy(leftRightWalls[nextLocation[0], nextLocation[1]]);
				break;
		}
		return nextLocation;
	}
	
    // Takes an array and randomizes the order of all the elements
	private void shuffleElements(int[] elementsToShuffle) {
		int temp = 0;
		int indexToSwapWith = 0;
		for (int i = 0; i < elementsToShuffle.Length-1; i++) {
			temp = elementsToShuffle[i];
			indexToSwapWith = Random.Range(i, elementsToShuffle.Length);
			elementsToShuffle[i] =  elementsToShuffle[indexToSwapWith];
			elementsToShuffle[indexToSwapWith] =  temp;
		}
	}
	
    // Takes an array and formats it in to a comma separated string enclosed in [] brackets
	private string arrayToString(int[] array) {
		string temp = "[";
		for (int i = 0; i < array.Length; i++) {
			temp = temp + array[i].ToString();
			if (i != array.Length-1) temp = temp + ", ";
		}
		return temp + "]";
	}
	
	// Update is called once per frame
	void Update () {
		//nothing to do, the grid already exists
	}
}
