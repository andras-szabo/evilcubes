using System.Collections.Generic;
using UnityEngine;

public class TestCollisionChecker : MonoBehaviour 
{
	private List<ICollidable> otherCubesNearby;

	public bool IsPathFree(IEnumerable<Vector3> path, float cubeSize)
	{
		foreach (var cube in otherCubesNearby)
		{
			if (cube.OverlapsAnyPosition(path, cubeSize))
			{
				return false; 
			}
		}

		return true;
	}
}
