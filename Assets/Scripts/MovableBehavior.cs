using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MovableBehavior : MonoBehaviour
{
	public MazeEngine MazeEngine;
	private Queue<Direction> movementsQueue;
	private const double MillissecondsBetweenMovements = 200;
	private DateTime lastMovementTime;

	// Use this for initialization
	public virtual void Start ()
	{
		MazeEngine = GameObject.Find ("MazeEngine").GetComponent<MazeEngine> ();	
		movementsQueue = new Queue<Direction> ();
		lastMovementTime = DateTime.Now;
	}

	public virtual void Update(){
		if (movementsQueue.Count > 0) {
			DateTime now = DateTime.Now;
			TimeSpan timeFromLastMovement = now - lastMovementTime;
			//Debug.Log("Time from last movement: " + timeFromLastMovement.TotalMilliseconds);

			if (timeFromLastMovement.TotalMilliseconds >= (double)MillissecondsBetweenMovements) {
				Direction direction = movementsQueue.Dequeue();
				PreMovementAction(direction);
				MazeEngine.TryToMove (gameObject, direction);
				PosMovementAction(direction);
				lastMovementTime = now;
			}
		}
	}

	public virtual void PosMovementAction(Direction direction){
	}

	public virtual void PreMovementAction(Direction direction){
	}

	public virtual void Move (Direction direction)
	{
		movementsQueue.Enqueue (direction);
	}

	public void ClearMovementQueue ()
	{
		movementsQueue.Clear();
	}

	public void RandomMove ()
	{
		Move (DirectionExtensions.RandomDirection);
	}
}
