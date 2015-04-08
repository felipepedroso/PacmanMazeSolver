using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SearchButtonsBehavior : MonoBehaviour
{
	public void DoDfsClick ()
	{
		PacmanBehavior pacmanBehavior = GetPacmanBehavior ();
		
		if (pacmanBehavior != null) {
			pacmanBehavior.DoDFS ();
		}
	}

	public PacmanBehavior GetPacmanBehavior ()
	{
		GameObject pacmanGameObject = GameObject.Find ("PacmanTile");
		PacmanBehavior pacmanBehavior = null;

		if (pacmanGameObject != null) {
			pacmanBehavior = pacmanGameObject.GetComponent<PacmanBehavior> ();
		}
		return pacmanBehavior;
	}

	public void DoBfsClick ()
	{
		PacmanBehavior pacmanBehavior = GetPacmanBehavior ();
			
		if (pacmanBehavior != null) {
			pacmanBehavior.DoBFS ();
		}
	}
}
