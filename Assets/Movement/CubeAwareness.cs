using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class CubeAwareness : MonoBehaviour 
{
	public bool active;
	private List<CubeAwareness> _otherCubesNearby = new List<CubeAwareness>();
	private HashSet<int> _otherCubeIDS = new HashSet<int>();

	private void OnTriggerEnter(Collider other)
	{
		var otherAwareness = other.GetComponent<CubeAwareness>();
		if (active && otherAwareness != null && !_otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
		{
			Debug.LogWarning("Adding cube: " + other.gameObject.name);
			_otherCubesNearby.Add(otherAwareness);
			_otherCubeIDS.Add(otherAwareness.GetInstanceID());
			Debug.LogFormat("{0} {1}", Time.frameCount, _otherCubesNearby.Count);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		var otherAwareness = other.GetComponent<CubeAwareness>();
		if (active && otherAwareness != null && _otherCubeIDS.Contains(otherAwareness.GetInstanceID()))
		{
			Debug.LogWarning("Removing cube: " + other.gameObject.name);
			_otherCubesNearby.Remove(otherAwareness);
			_otherCubeIDS.Remove(otherAwareness.GetInstanceID());
			Debug.LogFormat("{0} {1}", Time.frameCount, _otherCubesNearby.Count);
		}
	}

}
