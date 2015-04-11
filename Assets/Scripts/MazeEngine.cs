using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MazeEngine : MonoBehaviour
{
	public List<Layer> layers;

	public bool IsSquare;
	public bool RemoveDeadEnds;

	private int width, height;
	public int MaxWidth, MaxHeight;
	private const int MinDimensionSize = 3;

	public const string MazeLayerName = "MazeLayer";
	public const string PacmanLayerName = "PacmanLayer";

	MazeLayer mazeLayer;
	Layer pacmanLayer;

	private GameObject pacdotGameObject;

	// Use this for initialization
	void Start ()
	{
		ResetScene ();
	}

	public void ResetScene ()
	{
		if (layers != null) {
			foreach (var layer in layers) {
				layer.Destroy();
			}
			layers = null;
		}

		layers = new List<Layer> ();

		if (IsSquare) {
			width = height = Random.Range (MinDimensionSize, MaxWidth);
		} else {
			width  = Random.Range (MinDimensionSize, MaxWidth);
			height = Random.Range (MinDimensionSize, MaxHeight);
		}

		Camera.main.SendMessage ("SetupCamera", new Int32Point (width, height));

		GenerateMaze ();
		CreatePacmanLayer ();
	}

	void GenerateMaze ()
	{
		mazeLayer = new MazeLayer (MazeLayerName, gameObject.transform.position, width, height);
		GameObjectUtils.AppendChild (gameObject, mazeLayer.LayerGameObject);
		mazeLayer.LayerGameObject.transform.position = Vector3.zero;

		layers.Add (mazeLayer);

		if (RemoveDeadEnds) {
			mazeLayer.RemoveDeadEnds();
		}
	}

	void CreatePacmanLayer ()
	{
		pacmanLayer = new Layer (PacmanLayerName, gameObject.transform.position, width, height);
		GameObjectUtils.AppendChild (gameObject, pacmanLayer.LayerGameObject);
		pacmanLayer.LayerGameObject.transform.position = Vector3.zero;

		GameObject PacmanPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacman");
		GameObject PacdotPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacdot");
		GameObject GhostPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Ghost");

		Int32Point pacmanPosition = pacmanLayer.GetRandomEmptyPoint ();
		pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanPosition);
		pacmanLayer.GetTileAt (pacmanPosition).GetComponent<PacmanBehavior> ().MazeEngine = this;

		Int32Point ghostPosition = pacmanLayer.GetRandomEmptyPoint ();
		pacmanLayer.AddTileFromPrefab (GhostPrefab, ghostPosition);
		pacmanLayer.GetTileAt (ghostPosition).GetComponent<GhostBehavior> ().MazeEngine = this;

		CreatePacdot ();

		layers.Add (pacmanLayer);
	}

	public void CreatePacdot(){
/*		pacdotGameObject = GameObject.Find (PacdotPrefab.name + Layer.TILE_SUFFIX);

		if (pacdotGameObject != null) {
			if (pacmanLayer != null) {
				pacmanLayer.RemoveTile(pacdotGameObject);
			}
			Destroy(pacdotGameObject);
		}

		int pacdotX, pacdotY;
		
		do {
			pacdotX = Random.Range (0, width);
			pacdotY = Random.Range (0, height);
		} while(!pacmanLayer.IsPositionEmpty(pacdotX, pacdotY));
		
		pacmanLayer.AddTileFromPrefab (PacdotPrefab, pacdotX, pacdotY);
		pacdotGameObject = pacmanLayer.GetTileAt (pacdotX, pacdotY);

		Graph.Target = new Int32Point (pacdotX, pacdotY);*/
	}

	public Layer GetLayerByName(string layerName){
		foreach (var layer in layers) {
			if (layer.Name.Equals(layerName)) {
				return layer;
			}
		}

		return null;
	}

	public Layer GetGameObjectLayer(GameObject gameObject){
		foreach (var layer in layers) {
			if (layer.ContainsTile(gameObject)) {
				return layer;
			}
		}

		return null;
	}

	public void TryToMove (GameObject gameObject, Direction direction)
	{
		Layer gameObjectLayer = GetGameObjectLayer (gameObject);

		if (gameObjectLayer != null && gameObjectLayer.ContainsTile(gameObject)) {
			Int32Point gameObjectPosition = gameObjectLayer.GetTilePosition(gameObject);
			Int32Point directionPoint = direction.ToInt32Point();

			if (!mazeLayer.HasWallBetween(gameObjectPosition, gameObjectPosition + directionPoint)) {
				gameObjectLayer.MoveTo(gameObject, gameObjectPosition + directionPoint);				
			}
		}
	}
}
