using UnityEngine;
using System.Collections;

public static class DirectionExtensions
{
	private static Int32Point[] directions = {
		new Int32Point (0, 1), // Up
		new Int32Point (1, 0), // Right
		new Int32Point (0, -1),// Down
		new Int32Point (-1, 0) // Left
	};

	public static Direction RandomValue {
		get {
			return (Direction)Random.Range(0, directions.Length);
		}
	}

	public static Int32Point ToInt32Point (this Direction direction) {
		return directions[(int)direction];
	}
}
