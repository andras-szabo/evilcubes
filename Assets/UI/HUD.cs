using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	public const float CROSSHAIR_MIN_SCALE = 0.1f;
	public const float CROSSHAIR_MAX_SCALE = 2f;

	public Text activeWeaponLabel;
	public Image activeWeaponCooldown;
	public RectTransform crosshair;

	private PlayerController _player;
	private WeaponController.WeaponState _weaponState;
	private bool _isSetup;

	private IEnumerator Start()
	{
		while (_player == null)
		{
			_player = ManagerLocator.TryGet<PlayerController>();
			if (_player == null)
			{
				yield return null;
			}
		}

		Setup();
	}

	private void Update()
	{
		if (_isSetup)
		{
			UpdateWeaponCooldown();
		}
	}

	private void OnDestroy()
	{
		if (_player != null)
		{
			_player.weaponController.OnWeaponChanged -= HandleWeaponChanged;
			_player.weaponController.OnDispersionChanged -= HandleDispersionChanged;
		}
	}

	private void Setup()
	{
		if (_player != null)
		{
			var wc = _player.weaponController;
			HandleWeaponChanged(wc.CurrentWeaponState);
			HandleDispersionChanged(wc.CurrentWeaponState.currentDispersionDegrees);

			wc.OnWeaponChanged += HandleWeaponChanged;
			wc.OnDispersionChanged += HandleDispersionChanged;
			
			_isSetup = true;
		}
	}

	private void HandleDispersionChanged(float currentDispersionDegrees)
	{
		var scaleFactor = Mathf.Lerp(CROSSHAIR_MIN_SCALE, CROSSHAIR_MAX_SCALE, currentDispersionDegrees / WeaponController.MAX_DISPERSION_DEGREE);
		crosshair.localScale = new Vector3(scaleFactor, scaleFactor, 1f);
	}

	private void HandleWeaponChanged(WeaponController.WeaponState weaponState)
	{
		_weaponState = weaponState;
		UpdateWeaponName();
		UpdateWeaponCooldown();
	}

	private void UpdateWeaponName()
	{
		activeWeaponLabel.text = _weaponState.name;
	}

	private void UpdateWeaponCooldown()
	{
		var cooldownRate = Mathf.Min(1f, _weaponState.elapsedSinceLastShot / _weaponState.cooldown);
		activeWeaponCooldown.fillAmount = 1f - cooldownRate;
		activeWeaponCooldown.color = Color.Lerp(Color.green, Color.red, 1f - cooldownRate);
	}
}
