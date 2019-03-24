using UnityEngine;

public class PlayerController : MonoBehaviour
{
	public CameraRotator camRotator;
	public WeaponController weaponController;

	private void Awake()
	{
		//TODO
		//GameController.TryRegister<IPlayerController>(this);
	}

	private void Update()
	{
		ProcessWeaponInputs();
	}
	
	private void LateUpdate()
	{
		ProcessCameraRotationInput();
		ProcessCamPerspectiveSwitchInput();
	}

	private void ProcessWeaponInputs()
	{
		if (Input.GetMouseButtonDown(0))
		{
			weaponController.HandleTriggerPull();
		}
		else if (Input.GetMouseButton(0))
		{
			weaponController.HandleTriggerHeld();
		}
		else if (Input.GetMouseButtonUp(0))
		{
			weaponController.HandleTriggerLetGo();
		}
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
