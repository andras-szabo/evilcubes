using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
	public Text activeWeaponLabel;
	public Image activeWeaponCooldown;

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
		}
	}

	private void Setup()
	{
		if (_player != null)
		{
			HandleWeaponChanged(_player.weaponController.CurrentWeaponState);
			_player.weaponController.OnWeaponChanged += HandleWeaponChanged;
		}

		_isSetup = true;
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
