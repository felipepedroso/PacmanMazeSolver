using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

public class PacmanBehavior : MonoBehaviour {
	public float Speed;


	private Maze maze;

	List<Int32Point> path;

	GameObject dfsButtonGameObject;

	UnityAction dfsUnityAction;

	// Use this for initialization
	void Start () {
		maze = (Maze)FindObjectOfType<Maze> ();
	}

	// Update is called once per frame
	void Update () {
		if (!LeanTween.isTweening (gameObject)) {
			gameObject.GetComponent<Animator>().enabled = false;

			if (Input.GetKeyUp (KeyCode.Q)) {
				DoDFS();
			}
			if (Input.GetKeyUp (KeyCode.W)) {
				DoBFS();
			}

			if (path != null && path.Count > 0) {
				maze.PacmanLayer.MoveThroughPath (gameObject, path, Speed);
				path = null;
			}
		} else {
			gameObject.GetComponent<Animator>().enabled = true;
		}
	}

	public void DoDFS(){
		path = maze.Graph.DepthFirstSearchPath (maze.PacmanLayer.GetTilePosition (gameObject));
	}

	public void DoBFS(){
		path = maze.Graph.BreadthFirstSearchShortestPath (maze.PacmanLayer.GetTilePosition (gameObject));
	}
}
