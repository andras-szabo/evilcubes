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
}
