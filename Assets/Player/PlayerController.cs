using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public CameraRotator camRotator;

	private void Awake()
	{
		//GameController.TryRegister<IPlayerController>(this);
	}

	private void LateUpdate()
	{
		ProcessCameraRotationInput();
		ProcessCamPerspectiveSwitchInput();
	}

	private void ProcessCamPerspectiveSwitchInput()
	{
		if (Input.GetKeyDown(KeyCode.C))
		{
			camRotator.TogglePerspective();
		}
	}

	private void ProcessCameraRotationInput()
	{
		var v = Input.GetAxis("Mouse Y");
		var h = Input.GetAxis("Mouse X");

		camRotator.ApplyInput(v, h);
	}

}
