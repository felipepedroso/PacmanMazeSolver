using UnityEngine;
using System.Collections;

public class AdjustCamera : MonoBehaviour {
	public void SetupCamera(Int32Point mazeDimension){
		int width = mazeDimension.X, height = mazeDimension.Y;

		float goX = width / 2 - (width % 2 == 0 ? 0.5f : 0);
		float goY = height / 2 - (height % 2 == 0 ? 0.5f : 0);
		
		transform.position = new Vector3 (goX, goY, transform.position.z);
	}
}
