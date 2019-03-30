using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RearViewMirror : MonoBehaviour 
{
	public Camera rearViewCamera;
	public Camera topDownViewCamera;

	private RectTransform _rt;
	private RectTransform CachedRectTransform
	{
		get
		{
			return _rt ?? (_rt = GetComponent<RectTransform>());
		}
	}

	private void Awake()
	{
		if (rearViewCamera != null)
		{
			rearViewCamera.enabled = false;
		}

		if (topDownViewCamera != null)
		{
			topDownViewCamera.enabled = false;
		}
	}

	private void Start()
	{
		var pixelRect = GetPixelRectForCamera();
		if (rearViewCamera != null)
		{
			rearViewCamera.pixelRect = pixelRect;
			rearViewCamera.enabled = true;
		}

		if (topDownViewCamera != null)
		{
			topDownViewCamera.pixelRect = pixelRect;
		}
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
