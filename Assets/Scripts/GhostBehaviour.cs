using System.Collections;
using System;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

public class GhostBehaviour : TagBehaviour
{
	public Color EvadingColor;
	public Color NormalColor;
	public GameObject Body;
	public EyeBehavior[] Eyes;
	public int MinimumPacmanDistance;
	DateTime paralysisStartTime;
	public double ParalysisTimeInSeconds;

	public enum GhostState
	{
		Walking,
		Chasing,
		Evading,
		Paralysed
	}

	public GhostState CurrentState { get; private set; }

	public bool IsParalysed {
		get{
			return CurrentState == GhostState.Paralysed;
		}
	}


	public override void Start ()
	{
		base.Start ();
		ChangeState (GhostState.Walking);
	}

	public override void Update ()
	{
		base.Update ();
		UpdateStateMachine ();
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
		case GhostState.Paralysed:
			Recover ();
			break;
		case GhostState.Walking:
		default:
			Walk ();
			break;
		}
	}

	public void SetColor (Color color)
	{
		Body.GetComponent<SpriteRenderer> ().color = color;
	}

	void ChangeState (GhostState newState)
	{
		if (CurrentState != newState) {
			GhostState PreviousState = CurrentState;
			CurrentState = newState;
			//Debug.Log (string.Format ("Switched from {0} to {1}", PreviousState, CurrentState));

			if (CurrentState == GhostState.Evading) {
				SetColor (EvadingColor);
			} else {
				SetColor (NormalColor);
			}

			Body.GetComponent<SpriteRenderer>().enabled = CurrentState != GhostState.Paralysed;
		}
	}

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
			if (IsPacmanNear ()) {
				ChangeState (GhostState.Chasing);
				return;
			} else {
				ChangeState (GhostState.Walking);
				return;
			}
		} 

		if (IsPacmanNear ()) {
			ClearMovementQueue ();
			EvadeFromTarget ();
		} else {
			RandomMove ();
		}
	}

	void Chase ()
	{
		if (IsPacmanInvencible ()) {
			ChangeState (GhostState.Evading);
			return;
		}

		if (Target == null) {
			ChangeState (GhostState.Walking);
			return;			
		}

		if (IsPacmanNear ()) {
			ChaseTarget ();
		} else {
			ChangeState (GhostState.Walking);
			return;
		}
	}

	void Recover ()
	{
		TimeSpan timeDelta = DateTime.Now - paralysisStartTime;
			
		if (timeDelta.TotalSeconds >= ParalysisTimeInSeconds) {
		
			if (IsPacmanInvencible()) {
				ChangeState(GhostState.Evading);
				return;
			}else if (IsPacmanNear()) {
				ChangeState(GhostState.Chasing);
				return;
			} else{
				ChangeState(GhostState.Walking);
				return;
			}
		}

		RandomMove ();
	}

	public override void TryToMove (Direction direction)
	{
		if (CurrentState != GhostState.Paralysed) {
			base.TryToMove (direction);
			Vector3 scale = Body.GetComponent<SpriteRenderer>().transform.localScale;
			scale.x = -scale.x;
			Body.GetComponent<SpriteRenderer>().transform.localScale = scale;


		}
	}

	public void EnterParalysedMode ()
	{
		if (IsPacmanInvencible()) {
			ChangeState (GhostState.Paralysed);
			ClearMovementQueue();
			paralysisStartTime = DateTime.Now;
		}
	}

	bool IsPacmanNear ()
	{
		return Target != null && PathToTarget.Count < MinimumPacmanDistance && PathToTarget.Count > 0;
	}

	bool IsPacmanInvencible ()
	{
		return MazeEngine.IsPacmanInvencible ();
	}

	public override void PreMovementAction (Direction direction)
	{
		base.PreMovementAction (direction);
		if (Eyes != null) {
			foreach (var eye in Eyes) {
				eye.EyeDirection = direction;
			}
		}
	}
}
