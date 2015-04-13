using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class GhostBehavior : MovableBehavior
{
	public Color EvadingColor;
	public Color NormalColor;

	public List<Int32Point> PathToPacman { get; private set; }

	public enum GhostState
	{
		Walking,
		Chasing,
		Evading
	}

	public GhostState CurrentState { get; private set; }

	public override void Start ()
	{
		base.Start ();
		ChangeState (GhostState.Walking);
		PathToPacman = new List<Int32Point> ();
	}

	public override void Update ()
	{
		base.Update ();
		UpdateStateMachine ();
		PathToPacman = MazeEngine.GetPathFromPacmanToGhost (gameObject);
	}

	void UpdateStateMachine ()
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

	void SetColor(Color color){
		gameObject.GetComponent<SpriteRenderer> ().color = color;
	}

	void ChangeState (GhostState newState)
	{
		if (CurrentState != newState) {
			GhostState PreviousState = CurrentState;
			CurrentState = newState;

			if (CurrentState == GhostState.Evading) {
				SetColor(EvadingColor);
			}else{
				SetColor(NormalColor);
			}
			Debug.Log (string.Format ("Switched from {0} to {1}", PreviousState, CurrentState));
		}

	}

	private float deltaTime = 0;
	private Direction randomDirection;

	void Walk ()
	{
		if (IsPacmanInvencible ()) {
			ChangeState (GhostState.Evading);
			return;
		}

		if (IsPacmanNear ()) {
			ChangeState (GhostState.Chasing);
			return;
		}

		// Walk logic
		RandomMove ();
	}

	void Evade ()
	{
		if (!IsPacmanInvencible ()) {
			if (IsPacmanNear()) {
				ChangeState (GhostState.Chasing);
				return;
			}else{
				ChangeState (GhostState.Walking);
				return;
			}
		}

		// Evasion logic
	}

	void Chase ()
	{
		if (IsPacmanInvencible ()) {
			ChangeState (GhostState.Evading);
			return;
		}

		if (IsPacmanNear ()) {
			Int32Point nextCell = PathToPacman[PathToPacman.Count - 2];
			Int32Point currentPosition = MazeEngine.GetTilePosition(gameObject);
			base.ClearMovementQueue();
			Move((nextCell - currentPosition).ToDirectionEnum());
		} else {
			ChangeState (GhostState.Walking);
			return;
		}
	}

	bool IsPacmanNear ()
	{
		return PathToPacman.Count < 5 && PathToPacman.Count > 0;
	}

	bool IsPacmanInvencible ()
	{
		return MazeEngine.IsPacmanInvencible ();
	}

}
