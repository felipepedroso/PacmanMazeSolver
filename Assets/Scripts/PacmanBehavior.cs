using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PacmanBehavior : MovableBehavior
{
	public enum PacmanState
	{
		Evading,
		Chasing,
		SearchPacdots
	}

	public PacmanState CurrentState{ get; private set; }

	public bool InvencibleMode {
		get;
		set;
	}

	public bool IgnoreAI;
	DateTime invencibilityTimeStart;
	public double InvencibilityTimeInSeconds;
	private float[] Angles = {90, 0,270,180};

	public override void Start ()
	{
		base.Start ();
		CurrentState = PacmanState.SearchPacdots;
	}

	public override void Update ()
	{
		base.Update ();
		if (IgnoreAI) {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				Move (Direction.Up);
			}
			
			if (Input.GetKeyDown (KeyCode.DownArrow)) {
				Move (Direction.Down);
			}
			
			if (Input.GetKeyDown (KeyCode.RightArrow)) {
				Move (Direction.Right);
			}
			
			if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				Move (Direction.Left);
			}
			//return;
		}

		UpdateStateMachine ();		
	}

	void UpdateStateMachine ()
	{
		switch (CurrentState) {
		case PacmanState.Evading:
			Evade ();
			break;
		case PacmanState.Chasing:
			Chase ();
			break;
		case PacmanState.SearchPacdots:
		default:
			SearchPacdots ();
			break;
		}

	}

	void ChangeState (PacmanState newState)
	{
		if (CurrentState != newState) {
			PacmanState PreviousState = CurrentState;
			CurrentState = newState;
			Debug.Log (string.Format ("Switched from {0} to {1}", PreviousState, CurrentState));
		}

	}

	void Evade ()
	{
		if (InvencibleMode) {
			ChangeState (PacmanState.Chasing);
			return;
		} else {
			if (!IsGhostNear ()) {
				ChangeState (PacmanState.SearchPacdots);
				return;
			}
		}

		// Evading logic
	}

	void Chase ()
	{
		if (InvencibleMode) {
			TimeSpan timeDelta = DateTime.Now - invencibilityTimeStart;

			if (timeDelta.TotalSeconds >= InvencibilityTimeInSeconds) {
				InvencibleMode = false;
			}
		}

		if (!InvencibleMode) {
			if (IsGhostNear ()) {
				ChangeState (PacmanState.Evading);
				return;
			} else {
				ChangeState (PacmanState.SearchPacdots);
				return;
			}
		}

		// Chasing logic
	}

	void SearchPacdots ()
	{
		if (IsGhostNear () && !InvencibleMode) {
			ChangeState (PacmanState.Evading);
		}

		// Searching Pacdots


	}

	public void EnableInvencibleMode ()
	{
		InvencibleMode = true;
		ChangeState (PacmanState.Chasing);
		invencibilityTimeStart = DateTime.Now;
	}

	public Int32Point CurrentPositionInMaze{
		get{
			return MazeEngine.GetTilePosition(gameObject);
		}
	}

	bool ReachedPacdot ()
	{
		Int32Point currentPosition = CurrentPositionInMaze;

		if (currentPosition != null) {
			return MazeEngine.HasPacdotAt(currentPosition);
		}

		return false;
	}

	bool IsGhostNear ()
	{
		return false;
	}

	public override void PreMovementAction (Direction direction)
	{
		transform.eulerAngles = new Vector3 (0, 0, Angles [(int)direction]);
	}

	public override void PosMovementAction (Direction direction)
	{
		if (ReachedPacdot ()) {
			EnableInvencibleMode ();
			MazeEngine.DestroyPacdotAt(CurrentPositionInMaze);
		}
	}
}
