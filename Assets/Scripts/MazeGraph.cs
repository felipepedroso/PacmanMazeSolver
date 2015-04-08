using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class MazeGraph : Graph<Int32Point>
{
	public Int32Point Target { get; set; }

	public List<Int32Point> BreadthFirstSearchShortestPath (Int32Point pacmanPosition)
	{
		if (Target != null && GetNode(pacmanPosition) != null && GetNode(Target) != null) {
			return base.BreadthFirstSearchShortestPath (pacmanPosition, Target);
		} else {
			Debug.Log("No target!");
			return new List<Int32Point>();
		}
	}

	public List<Int32Point> DepthFirstSearchPath (Int32Point pacmanPosition)
	{
		if (Target != null && GetNode(pacmanPosition) != null  && GetNode(Target) != null) {
			return base.DepthFirstSearchPath (pacmanPosition, Target);
		} else {
			Debug.Log("No target!");
			return new List<Int32Point>();
		}
	}
	
}
