using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
	public List<Vector3> Path { get; set; }
	public List<Vector3> ProvisionalPath { get; private set; }

	public int lastFrameCheck;

	private NearbyCubeTracker _nearbyCubeTracker;
	private Transform _cachedTransform;
	private float _mySize = 1f;

	public PathFinder(Transform cachedTransform, NearbyCubeTracker tracker, float cubeHalfDiagonal)
	{
		_cachedTransform = cachedTransform;
		_nearbyCubeTracker = tracker;
		_mySize = cubeHalfDiagonal;

		ProvisionalPath = new List<Vector3>();
	}

	public bool AmIOverlappingAnotherCube(float halfEdge)
	{
		var myPosition = _cachedTransform.position;
		foreach (var other in _nearbyCubeTracker.OtherCubesNearby)
		{
			if (other.IsSpawning)
			{
				continue;
			}

			var dist = Vector3.SqrMagnitude(other.CachedTransform.position - myPosition);
			var min = Mathf.Pow(other.HalfEdgeSize + halfEdge, 2f);

			if (dist < min)
			{
				return true;
			}
		}

		return false;
	}

	public bool IsMyPositionInAnotherPath(out string name)
	{
		var path = new List<Vector3> { _cachedTransform.position };
		foreach (var otherCube in _nearbyCubeTracker.OtherCubesNearby)
		{
			if (otherCube.OverlapsAnyPositions(path, _mySize))
			{
				name = otherCube.gameObject.name;
				return true;
			}
		}

		name = "";
		return false;
	}

	public bool IsPathFree(List<Vector3> path)
	{
		ProvisionalPath.Clear();
		ProvisionalPath.AddRange(path);

		lastFrameCheck = Time.frameCount;

		foreach (var otherCube in _nearbyCubeTracker.OtherCubesNearby)
		{
			if (otherCube.OverlapsAnyPositions(path, _mySize))
			{
				return false;
			}
		}

		return true;
	}

	public bool OverlapsAnyPositions(List<Vector3> positions, float otherCubeSize)
	{
		var distanceLimitSquared = Mathf.Pow(otherCubeSize + _mySize, 2f);
		var myPos = _cachedTransform.position;
		foreach (var pos in positions)
		{
			if (Vector3.SqrMagnitude(myPos - pos) <= distanceLimitSquared)
			{
				return true;
			}

			if (Path != null)
			{
				foreach (var pathPoint in Path)
				{
					if (Vector3.SqrMagnitude(pathPoint - pos) <= distanceLimitSquared)
					{
						return true;
					}
				}
			}
		}

		return false;
	}

	public void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			if (Path != null && Path.Count > 0)
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
