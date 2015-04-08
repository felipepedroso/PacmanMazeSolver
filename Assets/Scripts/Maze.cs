using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class Maze : MonoBehaviour
{
	public bool IsSquare;
	public int MaxWidth, MaxHeight;
	public GameObject CameraGameObject;

	private int Width, Height;
	private Layer mazeLayer, pacmanLayer;
	private const int MinDimensionSize = 3;

	private GameObject PacmanPrefab, PacdotPrefab;

	public MazeGraph Graph {
		get;
		private set;
	}


	GameObject pacdotGameObject;

	public Layer PacmanLayer {
		get{ return pacmanLayer;}
	}

	private int TotalCells {
		get {
			return Width * Height;
		}
	}

	// Use this for initialization
	void Start ()
	{
		PacmanPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacman");
		PacdotPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Pacdot");

		ResetScene ();
	}

	public void ResetScene ()
	{
		if (pacmanLayer != null) {
			pacmanLayer.Destroy ();
			pacmanLayer = null;
		}
		if (mazeLayer != null) {
			mazeLayer.Destroy ();
			mazeLayer = null;
		}

		if (IsSquare) {
			Width = Height = Random.Range (MinDimensionSize, MaxWidth);
		} else {
			Width  = Random.Range (MinDimensionSize, MaxWidth);
			Height = Random.Range (MinDimensionSize, MaxHeight);
		}

		GenerateMaze ();
		Graph = GenerateGraphPathFromMaze (new Int32Point(0,0));
		CreatePacmanLayer ();
		SetupCamera ();
	}

	void SetupCamera ()
	{
		float goX = Width / 2 - (Width % 2 == 0 ? 0.5f : 0);
		float goY = Height / 2 - (Height % 2 == 0 ? 0.5f : 0);
		CameraGameObject.transform.position = new Vector3 (goX, goY, CameraGameObject.transform.position.z);
	}

	void CreatePacmanLayer ()
	{
		pacmanLayer = new Layer ("PacmanLayer", gameObject.transform.position, Width, Height);
		GameObjectUtils.AppendChild (gameObject, pacmanLayer.LayerGameObject);
		pacmanLayer.LayerGameObject.transform.position = Vector3.zero;

		int pacmanX = Random.Range (0, Width);
		int pacmanY = Random.Range (0, Height);
		pacmanLayer.AddTileFromPrefab (PacmanPrefab, pacmanX, pacmanY);

		CreatePacdot ();
	}

	public void CreatePacdot(){
		pacdotGameObject = GameObject.Find (PacdotPrefab.name + Layer.TILE_SUFFIX);

		if (pacdotGameObject != null) {
			if (pacmanLayer != null) {
				pacmanLayer.RemoveTile(pacdotGameObject);
			}
			Destroy(pacdotGameObject);
		}

		int pacdotX, pacdotY;
		
		do {
			pacdotX = Random.Range (0, Width);
			pacdotY = Random.Range (0, Height);
		} while(!pacmanLayer.IsPositionEmpty(pacdotX, pacdotY));
		
		pacmanLayer.AddTileFromPrefab (PacdotPrefab, pacdotX, pacdotY);
		pacdotGameObject = pacmanLayer.GetTileAt (pacdotX, pacdotY);

		Graph.Target = new Int32Point (pacdotX, pacdotY);
	}

	public MazeGraph GenerateGraphPathFromMaze (Int32Point point)
	{
		MazeGraph pathGraph = new MazeGraph ();

		Stack<Int32Point> cells = new Stack<Int32Point> ();
		List<Int32Point> processedCells = new List<Int32Point> ();
		
		cells.Push (point);
		
		while (cells.Count > 0) {
			Int32Point cell = cells.Pop ();

			if (processedCells.Contains (cell)) {
				continue;
			}

			processedCells.Add (cell);
			List<Int32Point> neighbours = GetCellNeighbours (cell);
	
			foreach (Int32Point neighbour in neighbours) {
				if (!HasWallBetween (cell, neighbour)) {
					pathGraph.AddEdge (cell, neighbour);
					cells.Push (neighbour);
				}
			}
		}

		//Debug.Log (pathGraph);
		return pathGraph;
	}

	bool HasWallBetween (Int32Point cell1, Int32Point cell2)
	{
		Int32Point diff = cell1 - cell2;
		WallType wallType = WallType.None;

		if (diff.X != 0) {
			if (diff.X > 0) {
				wallType = WallType.Left;
			} else {
				wallType = WallType.Right;
			}
		} else if (diff.Y != 0) {
			if (diff.Y < 0) {
				wallType = WallType.Top;
			} else {
				wallType = WallType.Bottom;
			}
		}

		if (wallType != WallType.None) {
			return mazeLayer.LayerArray [cell1.X, cell1.Y].GetComponent<CellBehavior> ().IsWallStanding (wallType);
		}

		return false;
	}

	void GenerateEmptyMaze ()
	{
		if (mazeLayer != null) {
			mazeLayer.Destroy ();
		}

		mazeLayer = new Layer ("Maze", gameObject.transform.position, Width, Height);
		GameObjectUtils.AppendChild (gameObject, mazeLayer.LayerGameObject);
		mazeLayer.LayerGameObject.transform.position = Vector3.zero;

		GameObject cellPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Cell");

		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				mazeLayer.AddTileFromPrefab (cellPrefab, i, j);
			}
		}

	}
	
	private void GenerateMaze ()
	{
		GenerateEmptyMaze ();
		
		Stack<Int32Point> cellLocations = new Stack<Int32Point> ();
		Int32Point currentCell = new Int32Point (Random.Range (0, Width), Random.Range (0, Height));
		
		int visitedCells = 1;
		
		while (visitedCells != TotalCells) {
			List<Int32Point> intactNeighbors = GetIntactNeighbors (currentCell);
			
			if (intactNeighbors.Count > 0) {
				Int32Point randomNeighbor = intactNeighbors [Random.Range (0, intactNeighbors.Count)];
				
				KnockWallBetween (currentCell, randomNeighbor);
				cellLocations.Push (currentCell);
				currentCell = randomNeighbor;
				
				visitedCells++;
			} else {
				currentCell = cellLocations.Pop ();
			}
		}
	}
	
	private void KnockWallBetween (Int32Point cell1, Int32Point cell2)
	{
		GameObject[,] CellGrid = mazeLayer.LayerArray;

		Int32Point diff = cell1 - cell2;
		
		if (diff.X != 0) {
			if (diff.X > 0) {
				CellGrid [cell1.X, cell1.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Left);
				CellGrid [cell2.X, cell2.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Right);
			} else {
				CellGrid [cell1.X, cell1.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Right);
				CellGrid [cell2.X, cell2.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Left);
			}
		} else {
			if (diff.Y != 0) {
				if (diff.Y < 0) {
					CellGrid [cell1.X, cell1.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Top);
					CellGrid [cell2.X, cell2.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Bottom);
				} else {
					CellGrid [cell1.X, cell1.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Bottom);
					CellGrid [cell2.X, cell2.Y].GetComponent<CellBehavior> ().KnockWall (WallType.Top);
				}
			}
		}
	}

	private List<Int32Point> GetCellNeighbours (Int32Point cell)
	{
		List<Int32Point> neighbours = new List<Int32Point> ();

		if (cell != null) {
			int x = cell.X;
			int y = cell.Y;
			
			if (x > 0) {
				neighbours.Add (new Int32Point (x - 1, y));
			}
			
			if (x < Width - 1) {
				neighbours.Add (new Int32Point (x + 1, y));
			}
			
			if (y > 0) {
				neighbours.Add (new Int32Point (x, y - 1));
			}
			
			if (y < Height - 1) {
				neighbours.Add (new Int32Point (x, y + 1));
			}
		}

		return neighbours;
	}
	
	private List<Int32Point> GetIntactNeighbors (Int32Point cell)
	{
		List<Int32Point> neighbours = GetCellNeighbours (cell);
		List<Int32Point> intactNeighbors = new List<Int32Point> ();

		GameObject[,] CellGrid = mazeLayer.LayerArray;

		foreach (Int32Point neighbour in neighbours) {
			int x = neighbour.X;
			int y = neighbour.Y;

			if (CellGrid [x, y].GetComponent<CellBehavior> ().HasIntactWalls) {
				intactNeighbors.Add (neighbour);
			}
		}
		
		return intactNeighbors;
	}

	public void Update ()
	{
	}
}
