using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;
using System;

public class PacmanBehaviour : TagBehaviour
{
	public GameObject GhostGameObject;
	public List<GhostBehaviour> Ghosts;

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

		Target = MazeEngine.GetNearestGhost (gameObject);
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

		Target = MazeEngine.GetNearestGhost (gameObject);
		ChaseTarget ();
		// Chasing logic
	}

	void SearchPacdots ()
	{
		if (IsGhostNear () && !InvencibleMode) {
			ChangeState (PacmanState.Evading);
			return;
		}

		Target = MazeEngine.GetNearestPacdot (gameObject);
		ChaseTarget ();
	}

	public void EnableInvencibleMode ()
	{
		InvencibleMode = true;
		ChangeState (PacmanState.Chasing);
		invencibilityTimeStart = DateTime.Now;
	}

	bool IsGhostNear ()
	{
		foreach (var ghost in Ghosts) {
			if (MazeEngine.GetPathToTarget(gameObject,ghost.gameObject, null).Count < 5) {
				return true;
			}
		}

		return false;
	}

	public override void PreMovementAction (Direction direction)
	{
		transform.eulerAngles = new Vector3 (0, 0, Angles [(int)direction]);
	}


}
