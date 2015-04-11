using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {
	public MazeEngine MazePrefab;
	private MazeEngine Engine;

	// Use this for initialization
	void Start () {
		Engine = Instantiate (MazePrefab) as MazeEngine;
		Engine.gameObject.name = MazePrefab.name;
	}

	public void OnResetClick(){
		if (Engine != null) {
			Engine.ResetScene();
		}
	}
}
