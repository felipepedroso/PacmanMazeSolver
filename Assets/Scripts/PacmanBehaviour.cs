using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PacmanBehaviour : TagBehaviour
{
	public GameObject GhostGameObject;

	public List<Int32Point> PathToGhost {
		get;
		private set;
	}

	public enum PacmanState
	{
		Evading,
		Chasing,
		SearchPacdots
	}

	public PacmanState CurrentState{ get; private set; }

	public bool InvencibleMode {
		get;
		private set;
	}

	public bool IgnoreAI, AlwaysSearchingPacdots;
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

		/*if (IgnoreAI) {
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
		}*/

		if (InvencibleMode) {
			TimeSpan timeDelta = DateTime.Now - invencibilityTimeStart;
			
			if (timeDelta.TotalSeconds >= InvencibilityTimeInSeconds) {
				InvencibleMode = false;
			}
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
			CurrentState = AlwaysSearchingPacdots ? PacmanState.SearchPacdots : newState;
			//Debug.Log (string.Format ("Switched from {0} to {1}", PreviousState, CurrentState));
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

		ChooseTarget ();
		EvadeFromTarget ();
	}

	void Chase ()
	{
		if (!InvencibleMode) {
			if (IsGhostNear ()) {
				ChangeState (PacmanState.Evading);
				return;
			} else {
				ChangeState (PacmanState.SearchPacdots);
				return;
			}
		}

		ChooseTarget ();
		ChaseTarget ();
		// Chasing logic
	}

	void SearchPacdots ()
	{
		if (IsGhostNear () && !InvencibleMode) {
			ChangeState (PacmanState.Evading);
		}

		ChooseTarget ();
		ChaseTarget ();
	}

	public void EnableInvencibleMode ()
	{
		InvencibleMode = true;
		ChangeState (PacmanState.Chasing);
		invencibilityTimeStart = DateTime.Now;
	}

	void ChooseTarget ()
	{
		//Debug.Log("Current state: " + CurrentState);
		if (CurrentState == PacmanState.SearchPacdots) {
			Target = MazeEngine.GetNearestPacdot (gameObject);
		} else {
			Target = MazeEngine.GetNearestGhost (gameObject);
		}
		//Debug.Log("Current target: " + Target.name);
	}


	bool IsGhostNear ()
	{
		return PathToTarget.Count < 5 && PathToTarget.Count > 0;
	}

	public override void PreMovementAction (Direction direction)
	{
		transform.eulerAngles = new Vector3 (0, 0, Angles [(int)direction]);
	}

	public override void PosMovementAction (Direction direction)
	{
		Int32Point currentPosition = MazeEngine.GetTilePosition (gameObject);

		if (MazeEngine.HasPacdotAt (currentPosition)) {
			MazeEngine.DestroyPacdotAt (MazeEngine.GetTilePosition (gameObject));
			EnableInvencibleMode ();
		}

		if (MazeEngine.HasGhostAt(currentPosition) && InvencibleMode) {
			MazeEngine.CaptureGhost (currentPosition);
		}
	}

}
