using UnityEngine;

public enum CamType
{
	Main = 0,
	Rear = 1,
	TopDown = 2
}

[RequireComponent(typeof(Camera))]
public class CameraManagerClient : MonoWithCachedTransform
{
	public CamType camType;
	public Camera Camera { get; private set; }

	public Vector3 Position
	{
		get { return CachedTransform.position; }
		set { CachedTransform.position = value; }
	}

	public bool CanShake { get; set; }

	private void Start ()
	{
		Camera = GetComponent<Camera>();
		if (Camera != null)
		{
			ManagerLocator.TryGet<CameraManager>().Register((int)camType, this); 
		}

		CanShake = true;
	}
}
