using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MazeEngine : MonoBehaviour
{
	public bool IsSquare;
	public bool RemoveDeadEnds;

	public int Width{ get; private set; }
	public int Height{ get; private set; }

	public int MinWidth, MaxWidth, MinHeight,MaxHeight;

	public const string MazeLayerName = "MazeLayer";
	public const string PacmanLayerName = "PacmanLayer";
	public const string  PacdotLayerName = "PacdotLayer";

	private List<LayerBehaviour> layers;
	MazeLayer mazeLayer;
	LayerBehaviour pacmanLayer;
	LayerBehaviour pacdotLayer;

	private GhostBehaviour ghost;
	private PacmanBehaviour pacman;


	// Use this for initialization
	void Start ()
	{
		ResetScene ();
	}

	public GameObject CreateLayer(string name, GameObject layerPrefab){
		GameObject layerGameObject = (GameObject)Instantiate (layerPrefab,gameObject.transform.position,gameObject.transform.rotation);
		layerGameObject.name = name;
		return layerGameObject;
	}

	public GameObject CreateLayer(string name){
		GameObject layerPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Layer");
		return CreateLayer(name, layerPrefab);
	}

	public void ResetScene ()
	{
		if (layers != null) {
			foreach (var layer in layers) {
				Destroy(layer.gameObject);
			}
			layers = null;
		}

		layers = new List<LayerBehaviour> ();

		if (IsSquare) {
			Width = Height = Random.Range (MinWidth, MaxWidth);
		} else {
			Width  = Random.Range (MinWidth, MaxWidth);
			Height = Random.Range (MinHeight, MaxHeight);
		}

		Camera.main.SendMessage ("SetupCamera", new Int32Point (Width, Height));

		GenerateMaze ();
		CreatePacmanLayer ();
		CreatePacdotLayer ();
	}

	void GenerateMaze ()
	{
		GameObject mazeLayerPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/MazeLayer");
		mazeLayer = CreateLayer (MazeLayerName, mazeLayerPrefab).GetComponent<MazeLayer>();
		mazeLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, mazeLayer.gameObject);
		mazeLayer.gameObject.transform.position = Vector3.zero;

		layers.Add (mazeLayer);

		if (RemoveDeadEnds) {
			mazeLayer.RemoveDeadEnds();
		}
	}

	void CreatePacmanLayer ()
	{
		pacmanLayer = CreateLayer (PacmanLayerName).GetComponent<LayerBehaviour> ();
		pacmanLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, pacmanLayer.gameObject);
		pacmanLayer.gameObject.transform.position = Vector3.zero;

		GameObject PacmanPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacman");
		GameObject GhostPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Ghost");

		Int32Point pacmanPosition = pacmanLayer.GetRandomEmptyPoint ();
		pacman = pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanPosition).GetComponent<PacmanBehaviour> ();
		pacman.MazeEngine = this;

		Int32Point ghostPosition = pacmanLayer.GetRandomEmptyPoint ();
		ghost = pacmanLayer.AddTileFromPrefab (GhostPrefab, ghostPosition).GetComponent<GhostBehaviour> ();
		ghost.MazeEngine = this;

		ghost.Target = pacman.gameObject;
		pacman.Target = pacman.GhostGameObject = ghost.gameObject;
		pacman.Obstacles = new List<GameObject> ();
		pacman.Obstacles.Add (pacman.GhostGameObject);

		layers.Add (pacmanLayer);
	}

	public void CreatePacdotLayer(){
		pacdotLayer = CreateLayer (PacdotLayerName).GetComponent<LayerBehaviour> ();;
		pacdotLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, pacdotLayer.gameObject);
		pacdotLayer.gameObject.transform.position = Vector3.zero;

		GameObject PacdotPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacdot");
		List<Int32Point> emptyPositions = pacmanLayer.GetAllEmptyPositions();

		for (int i = 0; i < 10; i++) {
			if (emptyPositions == null || emptyPositions.Count <= 0) {
				break;
			}

			Int32Point pacdotPosition = emptyPositions[Random.Range(0,emptyPositions.Count)];
			emptyPositions.Remove(pacdotPosition);

			PacdotBehaviour pacdot = pacdotLayer.AddTileFromPrefab (PacdotPrefab, pacdotPosition).GetComponent<PacdotBehaviour> ();
			pacdot.PacmanGameObject = pacman.gameObject;
			pacdot.MazeEngine = this;

		}

		//Debug.Log ("Added tiles: " + pacmanLayer.GetAllTiles().Count);

		//pacdotLayer.GetTileAt (pacdotPosition).GetComponent<PacdotBehavior> ().MazeEngine = this;

		layers.Add (pacdotLayer);
	}

	public LayerBehaviour GetLayerByName(string layerName){
		foreach (var layer in layers) {
			if (layer.gameObject.name.Equals(layerName)) {
				return layer;
			}
		}

		return null;
	}

	public LayerBehaviour GetGameObjectLayer(GameObject gameObject){
		foreach (var layer in layers) {
			if (layer.ContainsTile(gameObject)) {
				return layer;
			}
		}

		return null;
	}

	public void TryToMove (GameObject gameObject, Direction direction)
	{
		LayerBehaviour gameObjectLayer = GetGameObjectLayer (gameObject);

		if (gameObjectLayer != null && gameObjectLayer.ContainsTile(gameObject)) {
			Int32Point gameObjectPosition = gameObjectLayer.GetTilePosition(gameObject);
			Int32Point directionPoint = direction.ToInt32Point();

			if (!mazeLayer.HasWallBetween(gameObjectPosition, gameObjectPosition + directionPoint)) {
				gameObjectLayer.MoveTo(gameObject, gameObjectPosition + directionPoint);				
			}
		}
	}

	public Int32Point GetTilePosition (GameObject gameObject)
	{
		Int32Point position = null;

		LayerBehaviour layer = GetGameObjectLayer (gameObject);

		if (layer != null) {
			position = layer.GetTilePosition(gameObject);
		}

		return position;
	}

	public bool HasPacdotAt (Int32Point position)
	{
		return pacdotLayer.GetTileAt (position) != null;
	}

	public void DestroyPacdotAt (Int32Point position)
	{
		if (position != null) {
			pacdotLayer.RemoveTileAt(position);
		}
	}

	public void DestroyTile(GameObject gameObject){
		LayerBehaviour gameObjectLayer = GetGameObjectLayer (gameObject);

		if (gameObjectLayer != null) {
			gameObjectLayer.RemoveTile(gameObject);

			if (gameObject != null) {
				gameObject.GetComponent<PacdotBehaviour>().MarkedToBeDestroyed = true;
			}
		}
	}

	public bool IsPacmanInvencible ()
	{
		return pacman != null ? pacman.InvencibleMode : false;
	}

	public List<Int32Point> GetPathToTarget (GameObject chaser, GameObject target, List<GameObject>  obstacles=null)
	{
		List<Int32Point> pathToTarget = new List<Int32Point> ();

		List<Int32Point> pathRestrictions = new List<Int32Point> ();

		if (obstacles != null && obstacles.Count > 0) {
			foreach (var obstacle in obstacles) {
				Int32Point obstaclePosition = GetTilePosition(obstacle);

				if (obstaclePosition != null) {
					pathRestrictions.Add(obstaclePosition);
				}
			}
		}

		Int32Point chaserPosition = GetTilePosition (chaser);
		Int32Point targetPosition = GetTilePosition (target);

		if (chaserPosition != null && targetPosition != null) {
			pathToTarget = mazeLayer.Graph.AStarPath(chaserPosition, targetPosition, pathRestrictions);
		}

		return pathToTarget;
	}

	public GameObject GetNearestPacdot(GameObject seeker){
		GameObject nearestPacdot = null;

		if (seeker != null) {
			int minPathValue = 0;

			List<GameObject> tiles = pacdotLayer.GetAllTiles();
//			Debug.Log(tiles.Count);
			foreach (var pacdot in tiles) {
				List<Int32Point> pathToPacdot = GetPathToTarget(seeker, pacdot);
				if (nearestPacdot == null || pathToPacdot.Count < minPathValue) {
					minPathValue = pathToPacdot.Count;
					nearestPacdot = pacdot;
				}
			}
		}

		return nearestPacdot;
	}

	public Direction GetSaferDirection (GameObject gameObject, GameObject target)
	{
		Graph<Int32Point> graph = mazeLayer.Graph;
		
		Node<Int32Point> currentNode = graph.GetNode (GetTilePosition(gameObject));
		int currentPositionIndex = graph.Nodes.IndexOf (currentNode);

		Node<Int32Point> targetPosition = graph.GetNode (GetTilePosition(target));
		int targetIndex = graph.Nodes.IndexOf (targetPosition);

		List<List<int>> distanceMatrix = mazeLayer.Graph.GetDistanceMatrix ();

		Int32Point saferNode = currentNode.Value;
		int saferNodeDistance = distanceMatrix [currentPositionIndex][targetIndex];
		List<Int32Point> saferNodes = new List<Int32Point> ();



		foreach (var neighbour in currentNode.Neighbours) {
			int neighbourIndex = graph.Nodes.IndexOf (neighbour);
			int neighbourDistance = distanceMatrix [neighbourIndex][targetIndex];

			if (neighbourDistance > saferNodeDistance) {
				//saferNode = neighbour.Value;
				saferNodes.Add(neighbour.Value);
				//saferNodeDistance = neighbourDistance;
			}
		}

		if (saferNodes.Count > 0) {
			saferNode = saferNodes[Random.Range(0, saferNodes.Count)];
		}

		return (saferNode - currentNode.Value).ToDirectionEnum();;
	}
}
