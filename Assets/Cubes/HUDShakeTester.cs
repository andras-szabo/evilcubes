using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDShakeTester : MonoBehaviour
{
	[Range(0.1f, 10f)] public float intensity;

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			foreach (var shakeable in ManagerLocator.TryGetAll<IShakeable>())
			{
				shakeable.Shake(intensity);
			}
		}
	}
}
