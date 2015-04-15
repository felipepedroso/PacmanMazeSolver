using UnityEngine;
using System.Collections;

public class CellBehaviour : MonoBehaviour {
	public GameObject LeftWall;
	public GameObject RightWall;
	public GameObject TopWall;
	public GameObject BottomWall;

	public bool HasIntactWalls{
		get{
			return IsWallStanding(WallType.Left) && IsWallStanding(WallType.Right) && IsWallStanding(WallType.Top) && IsWallStanding(WallType.Bottom);
		}
	}

	private GameObject GetWallObject(WallType type){
		GameObject wall = null;
		
		switch (type) {
		case WallType.Top:
			wall = TopWall;
			break;
		case WallType.Bottom:
			wall = BottomWall;
			break;
		case WallType.Left:
			wall = LeftWall;
			break;
		case WallType.Right:
			wall = RightWall;
			break;
		}

		return wall;
	}

	public bool IsWallStanding(WallType type){
		GameObject wall = GetWallObject (type);

		return wall != null ? wall.activeInHierarchy : false;
	}

	public void KnockWall(WallType type){
		GameObject wall = GetWallObject (type);

		if (wall != null) {
			wall.SetActive(false);
		}
	}
}
