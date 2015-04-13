using UnityEngine;
using System.Collections;
using System;

public static class DirectionExtensions
{
	private static Int32Point[] directions = {
		new Int32Point (0, 1), // Up
		new Int32Point (1, 0), // Right
		new Int32Point (0, -1),// Down
		new Int32Point (-1, 0) // Left
	};

	public static Direction RandomDirection {
		get {
			return (Direction)UnityEngine.Random.Range(0, directions.Length);
		}
	}

	public static Int32Point ToInt32Point (this Direction direction) {
		return directions[(int)direction];
	}

	public static Direction ToDirectionEnum(this Int32Point point){
		int x = point.X;
		int y = point.Y;

		int absX = Mathf.Abs(point.X);
		int absY = Mathf.Abs(point.Y);

		if (x > 0 && absX > absY) {
			return Direction.Right;
		}

		if (x < 0 && absX > absY) {
			return Direction.Left;
		}

		if (y > 0 && absY > absX) {
			return Direction.Up;
		}

		return Direction.Down;

	}
}
