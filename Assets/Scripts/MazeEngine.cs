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
	public int MinWidth, MaxWidth, MinHeight,MaxHeight;

	public const string MazeLayerName = "MazeLayer";
	public const string PacmanLayerName = "PacmanLayer";
	public const string  PacdotLayerName = "PacdotLayer";

	MazeLayer mazeLayer;
	Layer pacmanLayer;

	Layer pacdotLayer;

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
			width = height = Random.Range (MinWidth, MaxWidth);
		} else {
			width  = Random.Range (MinWidth, MaxWidth);
			height = Random.Range (MinHeight, MaxHeight);
		}

		Camera.main.SendMessage ("SetupCamera", new Int32Point (width, height));

		GenerateMaze ();
		CreatePacmanLayer ();
		CreatePacdotLayer ();
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
		GameObject GhostPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Ghost");

		Int32Point pacmanPosition = pacmanLayer.GetRandomEmptyPoint ();
		pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanPosition);
		pacmanLayer.GetTileAt (pacmanPosition).GetComponent<PacmanBehavior> ().MazeEngine = this;

		Int32Point ghostPosition = pacmanLayer.GetRandomEmptyPoint ();
		pacmanLayer.AddTileFromPrefab (GhostPrefab, ghostPosition);
		pacmanLayer.GetTileAt (ghostPosition).GetComponent<GhostBehavior> ().MazeEngine = this;

		layers.Add (pacmanLayer);
	}

	public void CreatePacdotLayer(){
		pacdotLayer = new Layer (PacdotLayerName, gameObject.transform.position, width, height);
		GameObjectUtils.AppendChild (gameObject, pacdotLayer.LayerGameObject);
		pacdotLayer.LayerGameObject.transform.position = Vector3.zero;

		GameObject PacdotPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacdot");

		for (int i = 0; i < 4; i++) {
			Int32Point pacdotPosition = pacmanLayer.GetRandomEmptyPoint ();
			pacdotLayer.AddTileFromPrefab (PacdotPrefab, pacdotPosition);
		}

		//pacdotLayer.GetTileAt (pacdotPosition).GetComponent<PacdotBehavior> ().MazeEngine = this;

		layers.Add (pacdotLayer);
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
