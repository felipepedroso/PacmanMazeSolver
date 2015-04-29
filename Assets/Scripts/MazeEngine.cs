using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MazeEngine : MonoBehaviour
{
	public bool IsSquare;
	public bool RemoveDeadEnds;
	public bool RandomizePositions;

	public int Width{ get; private set; }
	public int Height{ get; private set; }

	public int MinWidth, MaxWidth, MinHeight,MaxHeight;

	public const string MazeLayerName = "MazeLayer";
	public const string PacmanLayerName = "PacmanLayer";
	public const string  PacdotLayerName = "PacdotLayer";

	public const string GhostsLayerName = "GhostsLayer";

	private List<LayerBehaviour> layers;
	MazeLayer mazeLayer;
	LayerBehaviour pacmanLayer;
	LayerBehaviour pacdotLayer;
	LayerBehaviour ghostsLayer;

	private List<GhostBehaviour> ghosts;
	private List<GameObject> pacdots;
	private PacmanBehaviour pacman;

	private List<System.DateTime> pacdotRemovalTime;

	public double PacdotRespawTimeInSeconds;

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
		ghosts = new List<GhostBehaviour> ();
		pacdots = new List<GameObject> ();
		pacdotRemovalTime = new List<System.DateTime> ();

		if (IsSquare) {
			Width = Height = Random.Range (MinWidth, MaxWidth);
		} else {
			Width  = Random.Range (MinWidth, MaxWidth);
			Height = Random.Range (MinHeight, MaxHeight);
		}

		Camera.main.SendMessage ("SetupCamera", new Int32Point (Width, Height));

		GenerateMaze ();
		CreatePacmanLayer ();
		CreateGhostsLayer ();
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

	void CreateGhostsLayer ()
	{
		ghostsLayer = CreateLayer (GhostsLayerName).GetComponent<LayerBehaviour> ();
		ghostsLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, ghostsLayer.gameObject);
		ghostsLayer.gameObject.transform.position = Vector3.zero;
		
		GameObject GhostPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Ghost");

		Color[] colors = {Color.red, Color.magenta, Color.green, Color.blue};
		Int32Point[] positions = {new Int32Point(0,0), new Int32Point(Width - 1,0), new Int32Point(Width - 1,Height - 1), new Int32Point(0,Height - 1)};

		for (int i = 0; i < colors.Length; i++) {
			Int32Point ghostPosition = RandomizePositions ? pacmanLayer.GetRandomEmptyPoint () : positions[i];

			GhostBehaviour ghost = ghostsLayer.AddTileFromPrefab (GhostPrefab, ghostPosition).GetComponent<GhostBehaviour> ();
			ghost.MazeEngine = this;

			ghost.NormalColor = colors[i];
			ghost.SetColor(ghost.NormalColor);
			ghost.Obstacles = new List<GameObject> ();
			ghost.Target = pacman.gameObject;

			pacman.Obstacles.Add (pacman.GhostGameObject);
			ghosts.Add(ghost);
		}

		pacman.Ghosts.AddRange (ghosts);

		for (int i = 0; i < ghosts.Count; i++) {
			for (int j = i; j < ghosts.Count; j++) {
				if (i != j) {
					ghosts[i].Obstacles.Add(ghosts[j].gameObject);
					ghosts[j].Obstacles.Add(ghosts[i].gameObject);
				}
			}
		}
		layers.Add (ghostsLayer);
	}

	void CreatePacmanLayer ()
	{
		pacmanLayer = CreateLayer (PacmanLayerName).GetComponent<LayerBehaviour> ();
		pacmanLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, pacmanLayer.gameObject);
		pacmanLayer.gameObject.transform.position = Vector3.zero;

		GameObject PacmanPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacman");

		Int32Point pacmanPosition = RandomizePositions ?  pacmanLayer.GetRandomEmptyPoint () : new Int32Point (Width /2 , Height /2);
		pacman = pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanPosition).GetComponent<PacmanBehaviour> ();
		pacman.MazeEngine = this;
		pacman.Obstacles = new List<GameObject> ();
		pacman.Ghosts = new List<GhostBehaviour> ();

		layers.Add (pacmanLayer);
	}

	public void CreatePacdotLayer(){
		pacdotLayer = CreateLayer (PacdotLayerName).GetComponent<LayerBehaviour> ();;
		pacdotLayer.InitializeLayer (Width, Height);
		GameObjectUtils.AppendChild (gameObject, pacdotLayer.gameObject);
		pacdotLayer.gameObject.transform.position = Vector3.zero;

		List<Int32Point> emptyPositions = pacmanLayer.GetAllEmptyPositions();

		for (int i = 0; i < 8; i++) {
			if (emptyPositions == null || emptyPositions.Count <= 0) {
				break;
			}

			CreatePacdot(emptyPositions);
		}

		//Debug.Log ("Added tiles: " + pacmanLayer.GetAllTiles().Count);

		//pacdotLayer.GetTileAt (pacdotPosition).GetComponent<PacdotBehavior> ().MazeEngine = this;

		layers.Add (pacdotLayer);
	}



	public void CreatePacdot(List<Int32Point> emptyPositions){
		GameObject PacdotPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacdot");

		Int32Point pacdotPosition = emptyPositions[Random.Range(0,emptyPositions.Count)];
		emptyPositions.Remove(pacdotPosition);

		pacdots.Add(pacdotLayer.AddTileFromPrefab (PacdotPrefab, pacdotPosition));
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
		foreach (var pacdot in pacdotLayer.GetAllTiles()) {
			Int32Point pacdotPosition = GetTilePosition(pacdot);

			if (pacdotPosition.Equals(position)) {
				return true;
			}
		}

		return false;
	}

	public bool HasGhostAt (Int32Point position)
	{
		return ghostsLayer.GetTileAt (position) != null;
	}

	public bool HasPacmanAt (Int32Point position)
	{
		return pacmanLayer.GetTileAt (position) != null;
	}

	public void DestroyPacdotAt (Int32Point position)
	{
		if (position != null) {
			GameObject pacdotGameObject = pacdotLayer.GetTileAt(position);

			if (pacdotGameObject != null) {
				pacdots.Remove(pacdotGameObject);
				pacdotLayer.RemoveTile(pacdotGameObject);
				pacdotRemovalTime.Add(System.DateTime.Now);
			}
		}
	}

	void Update(){
		System.DateTime now = System.DateTime.Now;

		foreach (System.DateTime removalTime in pacdotRemovalTime.ToArray()) {
			if (now.Subtract(removalTime).TotalMilliseconds / 1000 >= PacdotRespawTimeInSeconds) {
				CreatePacdot(pacdotLayer.GetAllEmptyPositions());
				pacdotRemovalTime.Remove(removalTime);
			}
		}

		if (pacman != null) {
			Int32Point pacmanCurrentPosition = GetTilePosition (pacman.gameObject);
			
			if (HasPacdotAt(pacmanCurrentPosition)) {
				pacman.EnableInvencibleMode();
				DestroyPacdotAt(pacmanCurrentPosition);
			}
			
			if (HasGhostAt(pacmanCurrentPosition)) {
				if (IsPacmanInvencible()) {
					CaptureGhost(pacmanCurrentPosition);
				}else{
					if (!ghostsLayer.GetTileAt(pacmanCurrentPosition).GetComponent<GhostBehaviour>().IsParalysed) {
						KillPacman(pacmanCurrentPosition);	
					}
				}
			}	
		}

	}

	public void CaptureGhost (Int32Point position)
	{
		if (position != null) {
			GameObject ghostGameObject = ghostsLayer.GetTileAt(position);

			if (ghostGameObject != null) {
				ghostGameObject.GetComponent<GhostBehaviour>().EnterParalysedMode();
			}
		}
	}

	public void KillPacman (Int32Point position)
	{
		if (position != null) {
			GameObject pacmanGameObject = pacmanLayer.GetTileAt(position);
			
			if (pacmanGameObject != null) {
				pacmanLayer.RemoveTile(pacmanGameObject);
			}
		}
	}

	public void DestroyTile(GameObject gameObject){
		LayerBehaviour gameObjectLayer = GetGameObjectLayer (gameObject);

		if (gameObjectLayer != null) {
			gameObjectLayer.RemoveTile(gameObject);
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

	private GameObject GetNearestElement (GameObject seeker, List<GameObject> tiles){
		GameObject element = null;
		
		if (seeker != null) {
			int minPathValue = 0;
			
			//			Debug.Log(tiles.Count);
			foreach (var tile in tiles) {
				List<Int32Point> pathToElement = GetPathToTarget(seeker, tile);
				if (element == null || pathToElement.Count < minPathValue) {
					minPathValue = pathToElement.Count;
					element = tile;
				}
			}
		}
		
		return element;
	}

	public GameObject GetNearestGhost (GameObject seeker)
	{	
		GameObject nearestGhost = null;

		if (seeker != null) {
			int minPathValue = 0;

			foreach (var ghost in ghosts) {
				if (!ghost.IsParalysed) {
					List<Int32Point> pathToElement = GetPathToTarget(seeker, ghost.gameObject);
					if (nearestGhost == null || pathToElement.Count < minPathValue) {
						minPathValue = pathToElement.Count;
						nearestGhost = ghost.gameObject;
					}
				}
			}
		}

		return nearestGhost;
	}


	public GameObject GetNearestPacdot(GameObject seeker){
		return GetNearestElement (seeker, pacdotLayer.GetAllTiles());
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
				saferNode = neighbour.Value;
				saferNodes.Add(neighbour.Value);
				saferNodeDistance = neighbourDistance;
			}
		}

		if (saferNodes.Count > 0) {
			saferNode = saferNodes[Random.Range(0, saferNodes.Count)];
		}

		return (saferNode - currentNode.Value).ToDirectionEnum();;
	}
}
