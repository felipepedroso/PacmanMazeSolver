using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PacmanBehavior : MovableBehavior {
	private float[] Angles = {90, 0,270,180};
	// Use this for initialization
	void Start () {
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			Move(Direction.Up);
		}
		
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			Move(Direction.Down);
		}
		
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			Move(Direction.Right);
		}
		
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			Move(Direction.Left);
		}
	}

	public override void Move (Direction direction)
	{
		transform.eulerAngles = new Vector3 (0, 0, Angles [(int)direction]);
		base.Move (direction);
	}

}
