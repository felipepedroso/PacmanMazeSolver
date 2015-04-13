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

	private GhostBehavior ghost;
	private PacmanBehavior pacman;


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
		pacman = pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanPosition).GetComponent<PacmanBehavior> ();
		pacman.MazeEngine = this;

		Int32Point ghostPosition = pacmanLayer.GetRandomEmptyPoint ();
		ghost = pacmanLayer.AddTileFromPrefab (GhostPrefab, ghostPosition).GetComponent<GhostBehavior> ();
		ghost.MazeEngine = this;
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

	public Int32Point GetTilePosition (GameObject gameObject)
	{
		Int32Point position = null;

		Layer layer = GetGameObjectLayer (gameObject);

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

	public bool IsPacmanInvencible ()
	{
		return pacman != null ? pacman.InvencibleMode : false;
	}

	public List<Int32Point> AStarPath(Int32Point start, Int32Point end){
		Graph<Int32Point> graph = mazeLayer.Graph;

		Node<Int32Point> startNode = graph.GetNode (start);
		HashSet<Node<Int32Point>> nodesEvaluated = new HashSet<Node<Int32Point>> ();
		HashSet<Node<Int32Point>> nodesToEvaluate = new HashSet<Node<Int32Point>> ();
		nodesToEvaluate.Add (startNode);

		Dictionary<Int32Point,Int32Point> cameFrom = new Dictionary<Int32Point,Int32Point> ();

		Dictionary<Node<Int32Point>, int> g_score = new Dictionary<Node<Int32Point>, int> ();
		g_score [startNode] = 0;

		Dictionary<Node<Int32Point>, int> f_score = new Dictionary<Node<Int32Point>, int> ();
		f_score [startNode] = g_score [startNode] + CalculateHeuristicCost (start, end);

		Node<Int32Point> endNode = graph.GetNode (end);

		while (nodesToEvaluate.Count > 0) {
			Node<Int32Point> current = GetNodeWithLowestCost(nodesToEvaluate, f_score);
			mazeLayer.GetTileAt(current.Value).GetComponent<SpriteRenderer>().color = Color.black;
			if (current.Equals(endNode)) {
				//mazeLayer.GetTileAt(current.Value).GetComponent<SpriteRenderer>().color = Color.green;
				return ReconstructPath(cameFrom, end);
			}

			nodesToEvaluate.Remove(current);
			nodesEvaluated.Add(current);

			foreach (var neighbour in current.Neighbours) {
				if (nodesEvaluated.Contains(neighbour)) {
					continue;
				}

				int neighbourGCost = g_score[current] + 1;

				if (!nodesToEvaluate.Contains(neighbour) || neighbourGCost < g_score[neighbour]) {
					cameFrom[neighbour.Value] = current.Value;
					g_score[neighbour] = neighbourGCost;
					f_score [neighbour] = g_score [neighbour] + CalculateHeuristicCost (neighbour.Value, end);

					if (!nodesToEvaluate.Contains(neighbour)) {
						//mazeLayer.GetTileAt(current.Value).GetComponent<SpriteRenderer>().color = Color.gray;
						nodesToEvaluate.Add(neighbour);
					}
				}
			}
		}

		return new List<Int32Point> ();
	}

	List<Int32Point> ReconstructPath (Dictionary<Int32Point, Int32Point> cameFrom, Int32Point end)
	{
		List<Int32Point> path = new List<Int32Point> ();
		path.Add (end);
		
		while (cameFrom.ContainsKey(end)) {
			end = cameFrom[end];
			path.Add (end);
		}

		return path;
	}

	Node<Int32Point> GetNodeWithLowestCost (HashSet<Node<Int32Point>> nodesToEvaluate, Dictionary<Node<Int32Point>, int> f_score)
	{
		Node<Int32Point> nodeWithLowestScore = null;
			
		foreach (var node in nodesToEvaluate) {
			if (nodeWithLowestScore == null || f_score[node] < f_score[nodeWithLowestScore]) {
				nodeWithLowestScore = node;
			}
		}

		return nodeWithLowestScore;
	}

	int CalculateHeuristicCost (Int32Point start, Int32Point end)
	{
		return Mathf.Abs (start.X - end.X) + Mathf.Abs (start.Y - end.Y); 
	}

	public List<Int32Point> GetPathFromPacmanToGhost(GameObject gameObject)
	{
		GameObject hunter = gameObject.Equals (pacman.gameObject) ? pacman.gameObject : ghost.gameObject;
		GameObject target = gameObject.Equals (pacman.gameObject) ? ghost.gameObject : pacman.gameObject;

		return AStarPath(pacmanLayer.GetTilePosition(hunter),pacmanLayer.GetTilePosition(target));
	}
}
