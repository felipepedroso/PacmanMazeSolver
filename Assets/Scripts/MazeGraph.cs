using UnityEngine;
using System.Collections;

public class MazeGraph : Graph<Int32Point>
{
	protected override int CalculateHeuristicCost (Int32Point start, Int32Point end)
	{
		return Mathf.Abs (start.X - end.X) + Mathf.Abs (start.Y - end.Y); 
	}
}
