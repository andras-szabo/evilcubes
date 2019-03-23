using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CubeAwareness : MonoBehaviour 
{
	private List<CubeAwareness> _otherCubesNearby = new List<CubeAwareness>();
	private HashSet<int> _otherCubeIDS = new HashSet<int>();

	private void OnTriggerEnter(Collider other)
	{
		var otherAwareness = other.GetComponent<CubeAwareness>();
		if (otherAwareness != null && !_otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
		{
			_otherCubesNearby.Add(otherAwareness);
			_otherCubeIDS.Add(otherAwareness.GetInstanceID());
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var otherAwareness = other.GetComponent<CubeAwareness>();
		if (otherAwareness != null && _otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
		{
			_otherCubesNearby.Remove(otherAwareness);
			_otherCubeIDS.Remove(otherAwareness.GetInstanceID());
		}
	}

}
