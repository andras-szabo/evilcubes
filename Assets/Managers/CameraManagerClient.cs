using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CamType
{
	Main = 0,
	Rear = 1,
	TopDown = 2
}

[RequireComponent(typeof(Camera))]
public class CameraManagerClient : MonoBehaviour
{
	public CamType camType;

	private void Start ()
	{
		var cam = GetComponent<Camera>();
		if (cam != null)
		{
			ManagerLocator.TryGet<CameraManager>().Register((int)camType, cam);
		}
	}
}
