using System;
using UnityEngine;

public class PlayerController : MonoBehaviour, IManager
{
	public event Action OnPlayerDead;

	public CameraRotator camRotator;
	public WeaponController weaponController;
	public HP HP;

	[Tooltip("When enabled, tapping 'W', 'A', or the right mouse button spins the player quickly around")]
	public bool allowQuickTurnaround;

	public bool Paused { get; set; }

	private void Awake()
	{
		ManagerLocator.TryRegister<PlayerController>(this);
		HP.OnHitPointsChanged += HandleHPChanged;
	}

	private void Start()
	{
		Setup();
	}

	private void Update()
	{
		if (!Paused)
		{
			ProcessWeaponInputs();
		}
	}

	private void LateUpdate()
	{
		if (!Paused)
		{
			ProcessCameraRotationInput();
			ProcessCamPerspectiveSwitchInput();
		}
	}

	private void OnDestroy()
	{
		HP.OnHitPointsChanged -= HandleHPChanged;
	}

	public void Setup()
	{
		Paused = false;
		HP.Reset();
	}

	private void HandleHPChanged(HPInfo info)
	{
		if (info.current <= 0)
		{
			OnPlayerDead?.Invoke();
		}
	}

	private void ProcessWeaponInputs()
	{
		if (Input.GetButtonDown("Fire1"))
		{
			weaponController.HandleTriggerPull();
		}
		else if (Input.GetButton("Fire1"))
		{
			weaponController.HandleTriggerHeld();
		}
		else if (Input.GetButtonUp("Fire1"))
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

		if (Input.GetKeyDown(KeyCode.R))
		{
			var hud = ManagerLocator.TryGet<HUD>();
			hud?.ToggleRearViewMirrorCamera();
		}
	}

	private void ProcessCameraRotationInput()
	{
		if (allowQuickTurnaround)
		{
			ProcessQuickTurnaroundInput();
		}

		var v = Input.GetAxis("Mouse Y");
		var h = Input.GetAxis("Mouse X");

		camRotator.ApplyInput(v, h);
	}

	private void ProcessQuickTurnaroundInput()
	{
		if (Input.GetButtonDown("Fire2"))
		{
			camRotator.DoQuickTurnaround();
		}
	}
}
