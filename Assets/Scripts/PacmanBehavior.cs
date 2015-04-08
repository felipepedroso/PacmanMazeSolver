using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PacmanBehavior : MonoBehaviour {
	public float Speed;
	private Maze maze;

	// Use this for initialization
	void Start () {
		maze = (Maze)FindObjectOfType<Maze> ();
	}

	// Update is called once per frame
	void Update () {
		if (!LeanTween.isTweening (gameObject)) {
			gameObject.GetComponent<Animator>().enabled = false;

			List<Int32Point> path = null;

			if (Input.GetKeyUp (KeyCode.Space)) {
				path = maze.Graph.DepthFirstSearchPath (maze.PacmanLayer.GetTilePosition (gameObject));
			}
			if (Input.GetKeyUp (KeyCode.A)) {
				path = maze.Graph.BreadthFirstSearchShortestPath (maze.PacmanLayer.GetTilePosition (gameObject));
			}

			if (path != null && path.Count > 0) {
				maze.PacmanLayer.MoveThroughPath (gameObject, path, Speed);
			}
		} else {
			gameObject.GetComponent<Animator>().enabled = true;
		}
	}
}
