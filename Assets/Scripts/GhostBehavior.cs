using System.Collections;
using System;
using System.Text;
using UnityEngine;

public class GhostBehavior : MovableBehavior
{
	//public bool isPacmanNear;
	//public bool isPacmanStronger;

	public enum GhostState
	{
		Walking,
		Chasing,
		Evading
	}

	public GhostState CurrentState { get; private set; }

	void Start ()
	{
		CurrentState = GhostState.Walking;
	}

	void Update ()
	{
		switch (CurrentState) {
		case GhostState.Chasing:
			Chase ();
			break;
		case GhostState.Evading:
			Evade ();
			break;
		case GhostState.Walking:
		default:
			Walk ();
			break;
		}
	}

	void ChangeState (GhostState newState)
	{
		GhostState PreviousState = CurrentState;
		CurrentState = newState;
		Debug.Log (string.Format ("Switched from {0} to {1}", PreviousState, CurrentState));
	}

	private float deltaTime = 0;
	private Direction randomDirection;

	void Walk ()
	{
		if (IsPacmanNear ()) {
			if (IsPacmanStronger ()) {
				ChangeState (GhostState.Evading);
				return;
			} else {
				ChangeState (GhostState.Chasing);
				return;
			}
		}

		// Walk logic
		if (deltaTime > 0.2) {
			deltaTime = 0;
			Direction[] directions = (Direction[])Enum.GetValues (typeof(Direction));
			randomDirection = directions [UnityEngine.Random.Range (0, directions.Length)];
			Move (randomDirection);
		} else {
			deltaTime += UnityEngine.Time.deltaTime;
		}
	}

	void Evade ()
	{
		if (!IsPacmanNear ()) {
			ChangeState (GhostState.Walking);
			return;
		} else {
			if (!IsPacmanStronger ()) {
				ChangeState (GhostState.Chasing);
				return;
			}
		}

		// Evasion Logic
	}

	void Chase ()
	{
		if (!IsPacmanNear ()) {
			ChangeState (GhostState.Walking);
			return;
		} else {
			if (IsPacmanStronger ()) {
				ChangeState (GhostState.Evading);
				return;
			}
		}

		// Chasing logic
	}

	bool IsPacmanNear ()
	{
		//return isPacmanNear;
		return false;
	}

	bool IsPacmanStronger ()
	{
		//return isPacmanStronger;
		return false;
	}

}
