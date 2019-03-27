using System.Collections.Generic;
using UnityEngine;

public class PathFinder
{
	public List<Vector3> Path { get; set; }
	public List<Vector3> ProvisionalPath { get; private set; }

	//TODO - remove debug
	public List<string> checkedCubes = new List<string>();
	public int lastFrameCheck;

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

	public bool AmIOverlappingAnotherCube()
	{
		foreach (var other in _nearbyCubeTracker.OtherCubesNearby)
		{
			var dist = Vector3.SqrMagnitude(other.transform.position - _cachedTransform.position);
			var min = Mathf.Pow(other.transform.localScale.x / 2f + _cachedTransform.localScale.x / 2f, 2f);

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
				//TODO remove
				//Debug.LogWarningFormat("{0} last checked for, in frame {1}:", otherCube.gameObject.name, otherCube.PathFinder.lastFrameCheck);
				//foreach (var other in otherCube.PathFinder.checkedCubes)
				//{
				//	Debug.LogWarning(other);
				//}
				name = otherCube.gameObject.name;
				return true;
			}
		}

		name = "";
		return false;
	}

	public bool IsPathFree(IEnumerable<Vector3> path)
	{
		ProvisionalPath.Clear();
		ProvisionalPath.AddRange(path);

		checkedCubes.Clear();
		foreach (var otherCube in _nearbyCubeTracker.OtherCubesNearby)
		{
			checkedCubes.Add(otherCube.gameObject.name);
		}
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
