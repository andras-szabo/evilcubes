using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
	public List<Vector3> Path { get; set; }
	public List<Vector3> ProvisionalPath { get; private set; }

	private NearbyCubeTracker _nearbyCubeTracker;
	private Transform _cachedTransform;
	private float _mySize = 1f;

	public PathFinder(Transform cachedTransform, NearbyCubeTracker tracker, float cubeHalfDiagonal)
	{
		_cachedTransform = cachedTransform;
		_nearbyCubeTracker = tracker;
		_mySize = cubeHalfDiagonal;

		Path = new List<Vector3>();
		ProvisionalPath = new List<Vector3>();
	}

	public bool IsPathFree(IEnumerable<Vector3> path)
	{
		ProvisionalPath.Clear();
		ProvisionalPath.AddRange(path);

		foreach (var otherCube in _nearbyCubeTracker.OtherCubesNearby)
		{
			if (otherCube.OverlapsAnyPositions(path, _mySize))
			{
				return false;
			}
		}

		return true;
	}

	public bool OverlapsAnyPositions(IEnumerable<Vector3> positions, float otherCubeSize)
	{
		var distanceLimitSquared = Mathf.Pow(otherCubeSize + _mySize, 2f);
		foreach (var pos in positions)
		{
			if (Vector3.SqrMagnitude(_cachedTransform.position - pos) <= distanceLimitSquared)
			{
				return true;
			}

			foreach (var pathPoint in Path)
			{
				if (Vector3.SqrMagnitude(pathPoint - pos) <= distanceLimitSquared)
				{
					return true;
				}
			}
		}

		return false;
	}

	public void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			if (Path.Count > 0)
			{
				Gizmos.color = Color.green;
				foreach (var pos in Path)
				{
					Gizmos.DrawWireSphere(pos, _mySize);
				}
			}
			else if (ProvisionalPath.Count > 0)
			{
				Gizmos.color = Color.cyan;
				foreach (var pos in ProvisionalPath)
				{
					Gizmos.DrawWireSphere(pos, _mySize);
				}

				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(_cachedTransform.position, _mySize);
			}
		}
	}
}
