using System.Collections;
using System;

public class GhostBehavior : MovableBehavior {
	private float deltaTime = 0;
	void Update () {
		if (deltaTime > 0.2) {
			deltaTime = 0;
			Direction[] directions = (Direction[])Enum.GetValues (typeof(Direction));
			Move (directions [UnityEngine.Random.Range (0, directions.Length)]);
		} else {
			deltaTime += UnityEngine.Time.deltaTime;
		}

	}
}
