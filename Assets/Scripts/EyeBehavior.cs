using UnityEngine;
using System.Collections;

public class EyeBehavior : MonoBehaviour
{
	public GameObject Pupil;
	public Direction EyeDirection;
	private const float MovementDelta = 0.22f;
	private Vector3[] EyeMovementDirections = {
		new Vector3 (0, MovementDelta),
		new Vector3 (MovementDelta, 0),
		new Vector3 (0, -MovementDelta),
		new Vector3 (-MovementDelta, 0),
		new Vector3 (0, 0)
	};
	// Use this for initialization
	void Start ()
	{
		EyeDirection = Direction.None;
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Pupil != null) {
			Pupil.transform.localPosition = EyeMovementDirections[(int)EyeDirection];
		}
	}
}
