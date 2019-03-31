﻿using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RearViewMirror : MonoBehaviour 
{
	private Camera rearViewCamera;
	private Camera topDownViewCamera;

	private RectTransform _rt;
	private RectTransform CachedRectTransform
	{
		get
		{
			return _rt ?? (_rt = GetComponent<RectTransform>());
		}
	}

	private void Start()
	{
		rearViewCamera = GameObject.FindGameObjectWithTag("RearViewCam").GetComponent<Camera>();
		topDownViewCamera = GameObject.FindGameObjectWithTag("TopDownViewCam").GetComponent<Camera>();

		var pixelRect = GetPixelRectForCamera();
		if (rearViewCamera != null)
		{
			rearViewCamera.pixelRect = pixelRect;
		}

		if (topDownViewCamera != null)
		{
			topDownViewCamera.pixelRect = pixelRect;
		}

		Setup();
	}

	public void Setup()
	{
		if (rearViewCamera != null) { rearViewCamera.enabled = true; }
		if (topDownViewCamera != null) { topDownViewCamera.enabled = false; }
	}

	public void DisableCameras()
	{
		if (rearViewCamera != null) { rearViewCamera.enabled = false; }
		if (topDownViewCamera != null) { topDownViewCamera.enabled = false; }
	}

	public void ToggleRearViewCamera()
	{
		if (rearViewCamera != null)
		{
			rearViewCamera.enabled = !rearViewCamera.enabled;
		}

		if (topDownViewCamera != null)
		{
			topDownViewCamera.enabled = !topDownViewCamera.enabled;
		}
	}

	public Rect GetPixelRectForCamera()
	{
		var worldCorners = GetWorldCorners();
		return new Rect(worldCorners[0], worldCorners[2] - worldCorners[0]);
	}

	public Vector3[] GetWorldCorners()
	{
		Vector3[] worldCorners = new Vector3[4];
		CachedRectTransform.GetWorldCorners(worldCorners);
		return worldCorners;
	}
}
