using UnityEngine;
using System.Collections;

public class MovableBehavior : MonoBehaviour {
	public MazeEngine MazeEngine;

	// Use this for initialization
	void Start () {
		MazeEngine = GameObject.Find ("MazeEngine").GetComponent<MazeEngine>();	
	}
	
	// Update is called once per frame
	void Update () {
	}

	public virtual void Move(Direction direction)
	{
		MazeEngine.TryToMove (gameObject, direction);
	}
}
