using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayerBehaviour : MonoBehaviour {
	private GameObject[,] layerGrid;
	public const string LAYER_SUFFIX = "Layer";
	public const string TILE_SUFFIX = "Tile";
	
	public int Width {
		get;
		private set;
	}
	
	public int Height {
		get;
		private set;
	}

	public int TotalCells {
		get {
			return Width * Height;
		}
	}
	
	public GameObject[,] LayerGrid {
		get {
			return layerGrid;
		}
	}

	public virtual void InitializeLayer(int width, int height){
		Width = width;
		Height = height;
		layerGrid = new GameObject[Width, Height];
	}
	
	public GameObject GetTileAt (Int32Point point)
	{
		if (layerGrid != null && IsInsideLayerBounds(point)) {
			return layerGrid [point.X, point.Y];
		}

		return null;
	}
	
	public GameObject GetTileAt (int x, int y)
	{
		return GetTileAt (new Int32Point (x, y));
	}
	
	public Int32Point GetTilePosition (GameObject tileGameObject)
	{
		if (tileGameObject != null && layerGrid != null) {
			for (int i = 0; i < Width; i++) {
				for (int j = 0; j < Height; j++) {
					if (tileGameObject.Equals (layerGrid [i, j])) {
						return new Int32Point (i, j);
					}
				}
			}
		}
		
		return null;
	}
	
	public bool IsPositionEmpty (Int32Point point)
	{
		return IsPositionEmpty (point.X, point.Y);
	}
	
	public bool IsPositionEmpty (int x, int y)
	{
		return layerGrid [x, y] == null;
	}
	
	public bool ContainsTile (GameObject tileGameObject)
	{
		if (tileGameObject != null) {
			return GetTilePosition (tileGameObject) != null;
		}
		
		return false;
	}
	
	public Vector3 CalculateRealCoordinates (Int32Point point)
	{
		return CalculateRealCoordinates (point.X, point.Y);
	}
	
	public Vector3 CalculateRealCoordinates (int x, int y)
	{
		Vector3 parentPosition = gameObject != null ? gameObject.transform.position : Vector3.zero;
		return new Vector3 (parentPosition.x + x, parentPosition.x + y);
	}
	
	public GameObject AddTileFromPrefab (GameObject prefabTile, Int32Point point)
	{
		return AddTileFromPrefab (prefabTile, point.X, point.Y);
	}
	
	public GameObject AddTileFromPrefab (GameObject prefabTile, int indexX, int indexY)
	{
		GameObject tileInstance = null;
		if (prefabTile != null) {
			tileInstance = (GameObject)Instantiate (prefabTile);
			tileInstance.name = prefabTile.name + TILE_SUFFIX;
			
			if (!AddTile (tileInstance, indexX, indexY)) {
				Destroy (tileInstance);
				return null;
			} 
		}
		
		return tileInstance;
	}
	
	public bool AddTile (GameObject tileGameObject, Int32Point point)
	{
		return AddTile (tileGameObject, point.X, point.Y);
	}
	
	public bool AddTile (GameObject tileGameObject, int x, int y)
	{
		if (tileGameObject != null && layerGrid != null && !ContainsTile (tileGameObject)) {
			if (IsInsideLayerBounds (x,y)) {
				layerGrid [x, y] = tileGameObject;
				tileGameObject.transform.position = CalculateRealCoordinates (x, y);
				GameObjectUtils.AppendChild (gameObject, tileGameObject);
				return true;
			}
		}
		
		return false;
	}
	
	public bool IsInsideLayerBounds (Int32Point position)
	{
		return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
	}

	public bool IsInsideLayerBounds (int x, int y)
	{
		return IsInsideLayerBounds (new Int32Point (x, y));
	}
	
	public bool RemoveTile (GameObject tileGameObject)
	{
		if (layerGrid != null) {
			if (ContainsTile (tileGameObject)) {
				Int32Point tilePosition = GetTilePosition (tileGameObject);
				DestroyImmediate (tileGameObject);
				layerGrid [tilePosition.X, tilePosition.Y] = null;
				Debug.Log ("Destroyed a object from layer " + gameObject.name);
				return true;
			}
		}
		return false;
	}
	
	public bool RemoveTileAt (Int32Point position)
	{
		return RemoveTile (GetTileAt (position));
	}
	
	private Vector3[] CreateVector3Path (Int32Point[] points)
	{
		Vector3[] path = new Vector3[points.Length];
		
		for (int i = 0; i < points.Length; i++) {
			path [i] = CalculateRealCoordinates (points [i].X, points [i].Y);
		}
		
		return path;
	}


	public void MoveTo (GameObject gameObject, Int32Point point, bool forceRemoval=false)
	{
		if (point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height) {
			return;
		}
		Int32Point tilePosition = GetTilePosition (gameObject);
		
		if (tilePosition.Equals (point)) {
			return;
		}
		
		if (IsPositionEmpty (point) || forceRemoval) {
			RemoveTile (layerGrid [point.X, point.Y]);
			layerGrid [point.X, point.Y] = layerGrid [tilePosition.X, tilePosition.Y];
			layerGrid [tilePosition.X, tilePosition.Y] = null;
			//gameObject.transform.position = CalculateRealCoordinates (point);
			LeanTween.move (gameObject, CalculateRealCoordinates (point), 0.2f);
		}
	}
	
	public List<Int32Point> GetCellNeighbours (Int32Point cell)
	{
		List<Int32Point> neighbours = new List<Int32Point> ();
		
		if (cell != null) {
			if (cell.X > 0) {
				neighbours.Add (cell + Direction.Left.ToInt32Point ());
			}
			
			if (cell.X < Width - 1) {
				neighbours.Add (cell + Direction.Right.ToInt32Point ());
			}
			
			if (cell.Y > 0) {
				neighbours.Add (cell + Direction.Down.ToInt32Point ());
			}
			
			if (cell.Y < Height - 1) {
				neighbours.Add (cell + Direction.Up.ToInt32Point ());
			}
		}
		
		return neighbours;
	}
	
	public Int32Point GetRandomEmptyPoint ()
	{
		List<Int32Point> emptyPositions = GetAllEmptyPositions ();
		
		if (emptyPositions.Count > 0) {
			int positionIndex = UnityEngine.Random.Range(0, emptyPositions.Count);
			return emptyPositions[positionIndex] ;
		}
		
		return null;
	}
	
	public List<Int32Point> GetAllEmptyPositions(){
		List<Int32Point> emptyPositions = new List<Int32Point> ();
		
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				if (LayerGrid [i, j] == null) {
					emptyPositions.Add(new Int32Point(i,j));
				}
			}
		}
		
		return emptyPositions;
	}
	
	public List<GameObject> GetAllTiles ()
	{
		List<GameObject> gameObjectsList = new List<GameObject> ();
		
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				if (LayerGrid [i, j] != null) {
					gameObjectsList.Add (LayerGrid [i, j]);
				}
			}
		}
		
		return gameObjectsList;
	}
}
