using UnityEngine;

public class PlayerController : MonoBehaviour, IManager
{
	public CameraRotator camRotator;
	public WeaponController weaponController;

	private void Awake()
	{
		ManagerLocator.TryRegister<PlayerController>(this);
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
		else
		{
			var mouseWheel = Input.GetAxis("Mouse ScrollWheel");
			if (!Mathf.Approximately(mouseWheel, 0f))
			{
				weaponController.CycleThroughWeapons(mouseWheel > 0f);
			}
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
