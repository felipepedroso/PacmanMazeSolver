//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.34014
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
// </auto-generated>
//------------------------------------------------------------------------------
using System;
using UnityEngine;
using System.Collections.Generic;

public class Layer
{
	private GameObject layerGameObject;
	private GameObject[,] layerArray;
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

	public GameObject[,] LayerArray {
		get {
			return layerArray;
		}
	}

	public GameObject LayerGameObject {
		get {
			return layerGameObject;
		}
	}

	public string Name {
		get { return LayerGameObject != null ? LayerGameObject.name : LAYER_SUFFIX; }
	}

	public int TotalCells {
		get {
			return Width * Height;
		}
	}
	
	public Layer (string layerName, Vector3 position, int width, int height)
	{
		Width = width;
		Height = height;
		layerArray = new GameObject[Width, Height];
			
		layerGameObject = new GameObject ();
		layerGameObject.name = layerName;
		layerGameObject.transform.position = position;
	}

	public void Destroy ()
	{
		if (layerGameObject != null) {
			MonoBehaviour.Destroy (layerGameObject);	
		}

		if (layerArray != null) {
			layerArray = null;
		}
	}

	public GameObject GetTileAt (Int32Point point){
		return GetTileAt (point.X, point.Y);
	}

	public GameObject GetTileAt (int x, int y)
	{
		if (layerArray != null) {
			return layerArray[x,y];
		}

		return null;
	}
	
	public Int32Point GetTilePosition (GameObject tileGameObject)
	{
		if (tileGameObject != null && layerArray != null) {
			for (int i = 0; i < Width; i++) {
				for (int j = 0; j < Height; j++) {
					if (tileGameObject.Equals (layerArray [i, j])) {
						return new Int32Point (i, j);
					}
				}
			}
		}

		return new Int32Point (-1, -1);
	}

	public bool IsPositionEmpty (Int32Point point){
		return IsPositionEmpty (point.X, point.Y);
	}

	public bool IsPositionEmpty (int x, int y)
	{
		return layerArray [x, y] == null;
	}

	public bool ContainsTile (GameObject tileGameObject)
	{
		if (tileGameObject != null) {
			Int32Point tilePosition = GetTilePosition (tileGameObject);		
				
			return (tilePosition.X != -1 && tilePosition.Y != -1);
		}

		return false;
	}

	public Vector3 CalculateRealCoordinates (Int32Point point){
		return CalculateRealCoordinates (point.X, point.Y);
	}

	public Vector3 CalculateRealCoordinates (int indexX, int indexY)
	{
		Vector3 parentPosition = layerGameObject != null ? layerGameObject.transform.position : Vector3.zero;
		return new Vector3 (parentPosition.x + indexX, parentPosition.x + indexY);
	}

	public GameObject AddTileFromPrefab(GameObject prefabTile, Int32Point point){
		return AddTileFromPrefab (prefabTile, point.X, point.Y);
	}

	public GameObject AddTileFromPrefab (GameObject prefabTile, int indexX, int indexY)
	{
		GameObject tileInstance = null;
		if (prefabTile != null) {
			tileInstance = (GameObject)MonoBehaviour.Instantiate (prefabTile);
			tileInstance.name = prefabTile.name + TILE_SUFFIX;

			if (!AddTile(tileInstance, indexX, indexY)) {
				MonoBehaviour.Destroy(tileInstance);
				return null;
			} 
		}

		return tileInstance;
	}

	public bool AddTile(GameObject gameObject, Int32Point point){
		return AddTile (gameObject, point.X, point.Y);
	}

	public bool AddTile(GameObject gameObject, int indexX, int indexY){
		if (gameObject != null && layerArray != null && !ContainsTile(gameObject)) {
			if (indexX >= 0 && indexX < Width && indexY >= 0 && indexY < Height) {
				layerArray [indexX, indexY] = gameObject;
				gameObject.transform.position = CalculateRealCoordinates (indexX, indexY);
				GameObjectUtils.AppendChild (layerGameObject, gameObject);
				return true;
			}
		}

		return false;
	}

	public bool RemoveTile (GameObject tileGameObject)
	{
		if (tileGameObject != null && layerArray != null) {
			if (ContainsTile(tileGameObject)) {
				Int32Point tilePosition = GetTilePosition(tileGameObject);
				layerArray [tilePosition.X, tilePosition.Y] = null;
				MonoBehaviour.Destroy(tileGameObject);
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

	public void MoveThroughPath (GameObject gameObject, List<Int32Point> path, float speed)
	{
		if (gameObject == null || path == null || path.Count <= 0 || speed <= 0) {
			return;
		}

		KeyValuePair<GameObject, Int32Point> finalPosition = new KeyValuePair<GameObject, Int32Point> (gameObject, path [path.Count - 1]);
		Vector3[] leanTweenPath = CreateVector3Path (path.ToArray ());
		if (leanTweenPath != null && leanTweenPath.Length > 0) {
			LeanTween.moveSpline (gameObject, leanTweenPath, path.Count / speed).setOrientToPath2d (true).setOnCompleteParam (finalPosition as object).setOnComplete (OnAnimationComplete);	
		}
	}
	public void OnAnimationComplete(object finalPosition){
		KeyValuePair<GameObject, Int32Point> positionToMove = (KeyValuePair<GameObject, Int32Point>)finalPosition ;
		MoveTo (positionToMove.Key, positionToMove.Value);
	}

	public void MoveTo (GameObject gameObject, Int32Point point, bool forceRemoval=false)
	{
		if ( point.X < 0 || point.X >= Width || point.Y < 0 || point.Y >= Height) {
			return;
		}
		Int32Point tilePosition = GetTilePosition (gameObject);

		if (tilePosition.Equals(point)) {
			return;
		}

		if (IsPositionEmpty(point) || forceRemoval) {
			RemoveTile (layerArray [point.X, point.Y]);
			layerArray [point.X, point.Y] = layerArray [tilePosition.X, tilePosition.Y];
			layerArray [tilePosition.X, tilePosition.Y] = null;
			//gameObject.transform.position = CalculateRealCoordinates (point);
			LeanTween.move (gameObject, CalculateRealCoordinates (point), 0.2f);
		}
	}

	public List<Int32Point> GetCellNeighbours (Int32Point cell)
	{
		List<Int32Point> neighbours = new List<Int32Point> ();
		
		if (cell != null) {
			if (cell.X > 0) {
				neighbours.Add(cell + Direction.Left.ToInt32Point());
			}
			
			if (cell.X < Width - 1) {
				neighbours.Add(cell + Direction.Right.ToInt32Point());
			}
			
			if (cell.Y > 0) {
				neighbours.Add(cell + Direction.Down.ToInt32Point());
			}
			
			if (cell.Y < Height - 1) {
				neighbours.Add(cell + Direction.Up.ToInt32Point());
			}
		}
		
		return neighbours;
	}

	public Int32Point GetRandomEmptyPoint(){
		Int32Point point;

		do {
			point = Int32Point.GenerateRandomPoint(0,0,Width, Height);
		} while(!IsPositionEmpty(point));

		return point;
	}

}
