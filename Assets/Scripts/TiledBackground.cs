using UnityEngine;
using System.Collections;

public class TiledBackground : MonoBehaviour {

	// The prefab to be used for the tiled images
	public GameObject topBannerRepeat;
	public GameObject topBanner;
	public GameObject[] topToBottomRepeatingBgs;
	public float bannerOffset = -1.0f;
	
	private GridMaker gridMaker;
	private const string GRID_OBJECT_NAME = "Grid";
	private const string ANTLION_OBJECT_NAME = "antlionbirdview";

	// Use this for initialization
	void Start () {
		gridMaker = GameObject.Find(GRID_OBJECT_NAME).GetComponent<GridMaker>();
		GameObject antlion = GameObject.Find(ANTLION_OBJECT_NAME);
		
		// Apply a set of tiled images
		float totalHeight = gridMaker.getTotalHeight() + Mathf.Abs(antlion.transform.position.y) + topBanner.renderer.bounds.size.y/4;
		float heightPerRepeatingImage;
		for (int i=0; i < topToBottomRepeatingBgs.Length; i++) {
			heightPerRepeatingImage = totalHeight/topToBottomRepeatingBgs.Length;
			applyTiledBackground(topToBottomRepeatingBgs[i], gridMaker.getTotalWidth(), (i+1)*heightPerRepeatingImage, antlion.transform.position.y + (i*heightPerRepeatingImage));
		}
		
		// Apply the banner to just above the exit of the maze
		GameObject banner = (GameObject) Instantiate(topBanner, new Vector2(0, gridMaker.getTotalHeight() + bannerOffset), Quaternion.identity);
		banner.transform.parent = transform;
		
		// Now apply as many sky bg's as we need to cover the rest of the above space
		applyTiledBackground(topBannerRepeat, gridMaker.getTotalWidth(), totalHeight + 5.0f, gridMaker.getTotalHeight() + bannerOffset);
	}
	
	// Update is called once per frame
	void Update () {
	}
	
	// Will apply our specified tiled background for the given dimensions
	// These dimensions specify the minimum area that needs to be covered, meaning that some excess will be present to the top/right sides
	public void applyTiledBackground(GameObject bgImage, float minWidth, float minHeight, float startingHeight) {
		Debug.Log("Tiling background <" + bgImage + "> for min width <" + minWidth + "> and min height <" + minHeight + ">");
		if (!bgImage) {
			Debug.LogWarning("No background image has been set!");
			return;
		}
		
		Vector2 tileSize = bgImage.GetComponent<SpriteRenderer>().bounds.size;
		int numOfColumns = (int) Mathf.Ceil( minWidth / tileSize.x );
		int numOfRows = (int) Mathf.Ceil( (minHeight-startingHeight) / tileSize.y );
		GameObject currentTile;
		Debug.Log("Tiling image with <" + numOfColumns + "> columns and <" + numOfRows + "> rows");
		for (int columnIndex = 0; columnIndex < numOfColumns; columnIndex++) {
			for (int rowIndex = 0; rowIndex < numOfRows; rowIndex++) {
				currentTile = (GameObject) Instantiate(bgImage, new Vector2(columnIndex * tileSize.x, startingHeight + (rowIndex * tileSize.y)), Quaternion.identity);
				currentTile.transform.parent = transform;
			}
		}
	}
	
	public GameObject getBannerImage() {
		return topBanner;
	}	
}
