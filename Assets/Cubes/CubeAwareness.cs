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
	private List<Vector3> _provisionalPath = new List<Vector3>();

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			if (_myPath.Count > 0)
			{
				Gizmos.color = gizmoColor;
				foreach (var pos in _myPath)
				{
					Gizmos.DrawWireSphere(pos, _mySize);
				}
			}
			else if (_provisionalPath.Count > 0)
			{
				Gizmos.color = Color.cyan;
				foreach (var pos in _provisionalPath)
				{
					Gizmos.DrawWireSphere(pos, _mySize);
				}

				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(CachedTransform.position, _mySize);
			}
		}
	}

	public void UpdateSize(float awarenessRadius, float maxStepDistance)
	{
		_mySize = awarenessRadius;
		var sphereCollider = GetComponent<Collider>() as SphereCollider;
		if (sphereCollider != null)
		{
			sphereCollider.radius = maxStepDistance * 2.25f;
		}
	}

	//TODO NOTE _myPath should not include current position,
	//		it's the path where I'm going to
	public void UpdatePath(List<Vector3> path)
	{
		_myPath = path;
	}

	public bool IsPathFree(IEnumerable<Vector3> path, float mySize, bool log)
	{
		if (log && L)
		{
			Debug.LogWarning("----");
		}

		_provisionalPath.Clear();
		_provisionalPath.AddRange(path);

		foreach (var otherCube in _otherCubesNearby)
		{
			if (log && L)
			{
				Debug.LogWarning(otherCube.gameObject.name);
			}

			if (otherCube.OverlapsAnyPosition(path, mySize, log))
			{
				return false;
			}
		}

		return true;
	}

	public bool OverlapsAnyPosition(IEnumerable<Vector3> positions, float otherCubeSize, bool log = false)
	{
		var distanceLimitSquared = Mathf.Pow(otherCubeSize + _mySize, 2f);
		if (log) { Debug.LogWarning(distanceLimitSquared); }

		foreach (var pos in positions)
		{
			if (Vector3.SqrMagnitude(CachedTransform.position - pos) <= distanceLimitSquared)
			{
				if (log) { Debug.LogWarningFormat("{0} pos", gameObject.name); }
				return true;
			}

			foreach (var pathPoint in _myPath)
			{
				if (Vector3.SqrMagnitude(pathPoint - pos) <= distanceLimitSquared)
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
