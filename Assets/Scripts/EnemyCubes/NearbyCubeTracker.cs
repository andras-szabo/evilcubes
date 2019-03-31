using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public class NearbyCubeTracker : MonoBehaviour
{
	private List<Enemy> _otherCubesNearby = new List<Enemy>();
	private HashSet<int> _otherCubeIDS = new HashSet<int>();

	private SphereCollider _sphereCollider;

	public List<Enemy> OtherCubesNearby
	{
		get
		{
			return _otherCubesNearby;
		}
	}

	public void UpdateTrackedAreaSize(float maxStepDistance)
	{
		_sphereCollider = GetComponent<SphereCollider>();
		if (_sphereCollider != null)
		{
			_sphereCollider.radius = maxStepDistance * 3f;
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.isTrigger)
		{
			var otherCube = other.GetComponent<Enemy>();
			if (otherCube != null && !_otherCubeIDS.Contains(otherCube.GetInstanceID()))
			{
				RegisterOther(otherCube);
			}
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.isTrigger)
		{
			var otherCube = other.GetComponent<Enemy>();
			if (otherCube != null && _otherCubeIDS.Contains(otherCube.GetInstanceID()))
			{
				UnregisterOther(otherCube);
			}
		}
	}

	private void RegisterOther(Enemy other)
	{
		_otherCubesNearby.Add(other);
		_otherCubeIDS.Add(other.GetInstanceID());

		Observe(other, true);
	}

	private void UnregisterOther(Enemy other)
	{
		_otherCubesNearby.Remove(other);
		_otherCubeIDS.Remove(other.GetInstanceID());

		Observe(other, false);
	}

	private void Observe(Enemy other, bool state)
	{
		other.OnRemoved -= HandleOtherCubeRemoved;

		if (state)
		{
			other.OnRemoved += HandleOtherCubeRemoved;
		}
	}

	private void HandleOtherCubeRemoved(Enemy cube)
	{
		UnregisterOther(cube);
	}

	private void OnDrawGizmos()
	{
		if (_sphereCollider != null)
		{
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireSphere(transform.position, _sphereCollider.radius);
		}	
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		foreach (var cube in _otherCubesNearby)
		{
			Gizmos.DrawLine(cube.transform.position, transform.position);
		}
	}
}
