using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IManager
{
	public CameraRotator camRotator;
	public WeaponController weaponController;
	public HP HP;

	[Tooltip("When enabled, tapping 'W', 'A', or the right mouse button spins the player quickly around")]
	public bool allowQuickTurnaround;

	private Coroutine _turnaroundRoutine;

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
			ProcessWeaponSelectionInputs();
		}
	}

	private void ProcessWeaponSelectionInputs()
	{
		var mouseWheel = Input.GetAxis("Mouse ScrollWheel");
		if (!Mathf.Approximately(mouseWheel, 0f))
		{
			weaponController.CycleThroughWeapons(mouseWheel < 0f);
		}
		else
		{
			var didSelectWeapon = false;
			for (int weaponIndex = 0; !didSelectWeapon && weaponIndex < weaponController.WeaponCount; ++weaponIndex)
			{
				if (Input.GetKeyDown(KeyCode.Alpha1 + weaponIndex) || Input.GetKeyDown(KeyCode.Keypad1 + weaponIndex))
				{
					didSelectWeapon = true;
					weaponController.SetActiveWeapon(weaponIndex);
				}
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
		if (allowQuickTurnaround)
		{
			ProcessQuickTurnaroundInput();
		}

		if (_turnaroundRoutine == null)
		{
			var v = Input.GetAxis("Mouse Y");
			var h = Input.GetAxis("Mouse X");

			camRotator.ApplyInput(v, h);
		}
	}

	private void ProcessQuickTurnaroundInput()
	{
		if (_turnaroundRoutine == null)
		{
			if (Input.GetMouseButton(1))
			{
				_turnaroundRoutine = StartCoroutine(QuickTurnAroundRoutine());
			}
		}
	}

	private IEnumerator QuickTurnAroundRoutine()
	{
		var forward = camRotator.CachedTransform.forward;
		var bodyForward = new Vector3(forward.x, 0f, forward.z).normalized;
		var startBodyRotation = Quaternion.LookRotation(bodyForward, Vector3.up);
		var targetBodyRotation = Quaternion.LookRotation(-bodyForward, Vector3.up);

		var elapsed = 0f;
		var duration = camRotator.turnAroundSeconds;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			var current = Quaternion.Slerp(startBodyRotation, targetBodyRotation, elapsed / duration);
			camRotator.body.rotation = current;
			yield return null;
		}

		camRotator.body.rotation = targetBodyRotation;
		
		_turnaroundRoutine = null;
	}
}
