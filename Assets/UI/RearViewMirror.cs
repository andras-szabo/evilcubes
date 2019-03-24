using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RearViewMirror : MonoBehaviour 
{
	public Camera rearViewCamera;

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
	}

	private void Start()
	{
		if (rearViewCamera != null)
		{
			rearViewCamera.pixelRect = GetPixelRectForCamera();
			rearViewCamera.enabled = true;
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
