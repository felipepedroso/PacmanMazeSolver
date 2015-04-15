using UnityEngine;
using System.Collections;

public class PacdotBehaviour : MonoBehaviour {
	public MazeEngine MazeEngine;
	public GameObject PacmanGameObject;

	public bool MarkedToBeDestroyed = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (MarkedToBeDestroyed) {
			Destroy(gameObject);
		}
	}


}
