﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MazeLayer : Layer
{
	public MazeLayer (string layerName, Vector3 position, int width, int height) : base(layerName,position,width,height)
	{
		RandomizeMaze ();
	}

	public bool HasWallBetween (Int32Point cell1, Int32Point cell2)
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
			return LayerArray [cell1.X, cell1.Y].GetComponent<CellBehavior> ().IsWallStanding (wallType);
		}
		
		return false;
	}

	void GenerateEmptyMaze ()
	{		
		GameObject cellPrefab = GameObjectUtils.GetPrefabFromResources ("Prefabs/Cell");
		
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				AddTileFromPrefab (cellPrefab, i, j);
			}
		}
	}

	private void RandomizeMaze ()
	{
		GenerateEmptyMaze ();
		
		Stack<Int32Point> cellLocations = new Stack<Int32Point> ();
		Int32Point currentCell = new Int32Point (Random.Range (0, Width), Random.Range (0, Height));
		
		int visitedCells = 1;
		
		while (visitedCells != TotalCells) {
			List<Int32Point> intactNeighbors = GetIntactNeighbours (currentCell);
			
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

	public void RemoveDeadEnds ()
	{
		for (int i = 0; i < Width; i++) {
			for (int j = 0; j < Height; j++) {
				Int32Point cell = new Int32Point (i, j);
				List<Int32Point> neighbours = GetCellNeighbours (cell);
				List<Int32Point> disconnectedNeighbours = new List<Int32Point> ();
				
				foreach (var neighbour in neighbours) {
					if (HasWallBetween (cell, neighbour)) {
						disconnectedNeighbours.Add (neighbour);
					}
				}
				
				if (disconnectedNeighbours.Count > 0) {
					if (disconnectedNeighbours.Count == neighbours.Count - 1) {
						KnockWallBetween (cell, disconnectedNeighbours [Random.Range (0, disconnectedNeighbours.Count)]);	
					}
				}
			}
		}
	}
	
	private void KnockWallBetween (Int32Point cell1, Int32Point cell2)
	{
		GameObject[,] CellGrid = LayerArray;
		
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

	private List<Int32Point> GetIntactNeighbours (Int32Point cell)
	{
		List<Int32Point> neighbours = GetCellNeighbours (cell);
		List<Int32Point> intactNeighbours = new List<Int32Point> ();
		
		foreach (Int32Point neighbour in neighbours) {
			int x = neighbour.X;
			int y = neighbour.Y;
			
			if (LayerArray [x, y].GetComponent<CellBehavior> ().HasIntactWalls) {
				intactNeighbours.Add (neighbour);
			}
		}
		
		return intactNeighbours;
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

}
