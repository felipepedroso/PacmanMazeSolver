using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Name inspired by: http://en.wikipedia.org/wiki/Tag_(game)
public class TagBehaviour : MovableBehaviour {
	public GameObject Target;
	public List<Int32Point> PathToTarget { get; private set; }
	public List<GameObject> Obstacles { get; set; }


	// Use this for initialization
	public override void Start (){
		base.Start ();
		PathToTarget = new List<Int32Point> ();
	}
	
	// Update is called once per frame
	public override void Update ()
	{
		base.Update ();

		if (Target != null) {
				PathToTarget = MazeEngine.GetPathToTarget (gameObject, Target,Obstacles);
		}
	}

	public virtual void ChaseTarget(){
		if (Target != null && PathToTarget != null && PathToTarget.Count > 1) {
			Int32Point nextCell =PathToTarget[PathToTarget.Count - 2];
			Int32Point currentPosition = MazeEngine.GetTilePosition(gameObject);
			base.ClearMovementQueue();
			Move((nextCell - currentPosition).ToDirectionEnum());
		}
	}

	public virtual void EvadeFromTarget(){
		if (Target != null) {
			base.ClearMovementQueue();
			Move(MazeEngine.GetSaferDirection(gameObject, Target));
		}
	}
}
