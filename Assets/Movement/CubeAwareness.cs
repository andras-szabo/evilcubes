using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CubeAwareness : MonoWithCachedTransform, ICollidable
{
	public Color gizmoColor;
	public bool L;

	public event CubeAwarenessDelegate OnRemoved;

	private List<CubeAwareness> _otherCubesNearby = new List<CubeAwareness>();
	private HashSet<int> _otherCubeIDS = new HashSet<int>();
	private float _mySize = 1f;

	private List<Vector3> _myPath = new List<Vector3>();

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = gizmoColor;
			foreach (var pos in _myPath)
			{
				Gizmos.DrawWireSphere(pos, _mySize);
			}
		}
	}

	//TODO NOTE _myPath should not include current position,
	//		it's the path where I'm going to
	public void UpdatePath(List<Vector3> path)
	{
		_myPath = path;
	}

	public bool IsPathFree(IEnumerable<Vector3> path, float cubeSize, bool log)
	{
		if (log)
		{
			Debug.LogWarning("----");
		}

		foreach (var cube in _otherCubesNearby)
		{
			if (log)
			{
				Debug.LogWarning(cube.gameObject.name);
			}

			if (cube.OverlapsAnyPosition(path, cubeSize))
			{
				return false;
			}
		}

		return true;
	}

	public bool OverlapsAnyPosition(IEnumerable<Vector3> positions, float cubeSize)
	{
		var distanceLimitSquared = (cubeSize + _mySize) * (cubeSize + _mySize);

		foreach (var pos in positions)
		{
			if (Vector3.SqrMagnitude(CachedTransform.position - pos) < distanceLimitSquared)
			{
				return true;
			}

			foreach (var pathPoint in _myPath)
			{
				if (Vector3.SqrMagnitude(pathPoint - pos) < distanceLimitSquared)
				{
					return true;
				}
			}

		}

		return false;
	}

	public bool OverlapsPosition(Vector3 position, float cubeSize)
	{
		//TODO
		return false;
	}

	private void RegisterOther(CubeAwareness other)
	{
		if (L)
		{
			Debug.LogWarning("Registering other: " + other.gameObject.name);
		}

		_otherCubesNearby.Add(other);
		_otherCubeIDS.Add(other.GetInstanceID());

		Observe(other, true);
	}

	private void UnregisterOther(CubeAwareness other)
	{
		if (L)
		{
			Debug.LogWarning("Unregistering other: " + other.gameObject.name);
		}

		_otherCubesNearby.Remove(other);
		_otherCubeIDS.Remove(other.GetInstanceID());

		Observe(other, false);
	}

	private void Observe(CubeAwareness other, bool state)
	{
		other.OnRemoved -= HandleOtherCubeRemoved;

		if (state)
		{
			other.OnRemoved += HandleOtherCubeRemoved;
		}
	}

	//TODO - instanceID maybe not needed
	private void HandleOtherCubeRemoved(CubeAwareness cube, int instanceID)
	{
		UnregisterOther(cube);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			var otherAwareness = other.GetComponent<CubeAwareness>();
			if (otherAwareness != null && !_otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
			{
				RegisterOther(otherAwareness);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (L)
		{
			Debug.LogWarningFormat("{0} vs {1}", gameObject.name, other.gameObject.name);
		}

		if (other.isTrigger)
		{
			var otherAwareness = other.GetComponent<CubeAwareness>();
			if (otherAwareness != null && _otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
			{
				UnregisterOther(otherAwareness);
			}
		}
	}

}
