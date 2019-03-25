﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoWithCachedTransform
{
	public class WeaponState
	{
		public WeaponState(WeaponConfig config)
		{
			name = config.id;
			cooldown = config.coolDownSeconds;
			elapsedSinceLastShot = float.MaxValue;
			currentDispersionDegrees = config.dispersionDegrees;
		}

		readonly public string name;
		readonly public float cooldown;

		public bool isActive;
		public float elapsedSinceLastShot;
		public float currentDispersionDegrees;
	}

	public const int MAX_HIT_PER_SHOT = 256;
	public const float MAX_DISPERSION_DEGREE = 15f;

	public event System.Action<WeaponState> OnWeaponChanged;
	public event System.Action<float> OnDispersionChanged;

	public WeaponConfig[] configs;

	private WeaponConfig _activeConfig;
	private int _activeConfigIndex;
	private WeaponState _activeWeaponState;
	private WeaponState[] _weaponStates;

	private RaycastHit[] _hits = new RaycastHit[MAX_HIT_PER_SHOT];
	private SortedList<float, RaycastHit> _hitsByDistance = new SortedList<float, RaycastHit>(); 

	private List<Vector3> _projectileRays = new List<Vector3>();
	private int _enemyLayerMask;
	private HitManager _hitManager;

	public WeaponState CurrentWeaponState
	{
		get
		{
			return _activeWeaponState;
		}
	}

	private void Awake()
	{
		_enemyLayerMask = LayerMask.GetMask("EvilCubes");
		
		_weaponStates = new WeaponState[configs.Length];
		for (int i = 0; i < configs.Length; ++i)
		{
			_weaponStates[i] = new WeaponState(configs[i]);
		}

		SetActiveWeapon(0);
	}

	private void Start()
	{
		_hitManager = ManagerLocator.TryGet<HitManager>();
	}

	private void LateUpdate()
	{
		foreach (var weaponState in _weaponStates)
		{
			weaponState.elapsedSinceLastShot += Time.deltaTime;
		}
	}

	public void SetActiveWeapon(int index)
	{
		if (index > -1 && index < configs.Length)
		{
			if (_activeWeaponState != null)
			{
				_activeWeaponState.isActive = false;
			}

			_activeConfigIndex = index;
			_activeConfig = configs[_activeConfigIndex];
			_activeWeaponState = _weaponStates[_activeConfigIndex];
			_activeWeaponState.isActive = true;

			ResetActiveWeaponDispersionRate();
			OnWeaponChanged?.Invoke(_activeWeaponState);
		}
	}

	public void CycleThroughWeapons(bool cycleUp)
	{
		var delta = cycleUp ? 1 : -1;

		_activeConfigIndex += delta;
		if (_activeConfigIndex < 0) { _activeConfigIndex = configs.Length - 1; }
		else { _activeConfigIndex %= configs.Length; }

		SetActiveWeapon(_activeConfigIndex);
	}

	public void HandleTriggerPull()
	{
		if (TryShoot())
		{
			if (_activeConfig.isAutomatic)
			{
				IncreaseDispersionRate();
			}
			else
			{
				StartCoroutine(SimulateRecoilDispersionRoutine(_activeWeaponState, _activeConfig.dispersionDegrees, 
															   _activeConfig.coolDownSeconds, simulateRecoil: true));
			}
		}
	}

	public void HandleTriggerHeld()
	{
		if (_activeConfig.isAutomatic)
		{
			if (TryShoot())
			{
				IncreaseDispersionRate();
			}
		}
	}

	public void HandleTriggerLetGo()
	{
		if (_activeConfig.isAutomatic)
		{
			StartCoroutine(SimulateRecoilDispersionRoutine(_activeWeaponState, _activeConfig.dispersionDegrees,
														   _activeConfig.coolDownSeconds, false));
		}
	}

	private bool TryShoot()
	{
		if (_activeWeaponState.elapsedSinceLastShot < _activeConfig.coolDownSeconds)
		{
			return false;
		}

		_activeWeaponState.elapsedSinceLastShot = 0f;
		_projectileRays.Clear();

		var origin = CachedTransform.position;

		for (int i = 0; i < _activeConfig.projectileCountPerShot; ++i)
		{
			var hDisplacement = 0f;
			var vDisplacement = 0f;

			CalculateBulletDisplacementAtUnitDistance(ref hDisplacement, ref vDisplacement);

			var aimDirection = CachedTransform.forward + CachedTransform.up * vDisplacement +
														 CachedTransform.right * hDisplacement;

			_projectileRays.Add(origin + aimDirection);

			var hitCount = Physics.RaycastNonAlloc(origin: origin, direction: aimDirection, results: _hits, maxDistance: _activeConfig.range,
												   layerMask: _enemyLayerMask);
			if (hitCount > 0)
			{
				ProcessHits(hitCount);
			}
		}
		return true;
	}

	private void CalculateBulletDisplacementAtUnitDistance(ref float hDisplacement, ref float vDisplacement)
	{
		var currentDispersionDegrees = _activeWeaponState.currentDispersionDegrees;
		var horizontalDispersion = Random.Range(-currentDispersionDegrees, currentDispersionDegrees);
		var verticalDispersion = Random.Range(-currentDispersionDegrees, currentDispersionDegrees);

		if (!Mathf.Approximately(horizontalDispersion, 0f))
		{
			hDisplacement = Mathf.Tan(Mathf.PI / 180f * horizontalDispersion);
		}

		if (!Mathf.Approximately(verticalDispersion, 0f))
		{
			vDisplacement = Mathf.Tan(Mathf.PI / 180f * verticalDispersion);
		}
	}

	private void ProcessHits(int hitCount)
	{
		_hitsByDistance.Clear();

		for (int i = 0; i < hitCount; ++i)
		{
			_hitsByDistance.Add(_hits[i].distance, _hits[i]);
		}

		var damage = _activeConfig.damagePerProjectile;

		for (int i = 0; i < _hitsByDistance.Count && damage > 0.1f; ++i)
		{
			var hit = _hitsByDistance.Values[i];
			_hitManager.ReportHit(hit.collider.gameObject, hit.point, damage);
			damage = (int) (damage * (1f - _activeConfig.dmgReductionRate));
		}
	}

	private void ResetActiveWeaponDispersionRate()
	{
		_activeWeaponState.currentDispersionDegrees = _activeConfig.dispersionDegrees;
		OnDispersionChanged?.Invoke(_activeWeaponState.currentDispersionDegrees);
	}

	private IEnumerator SimulateRecoilDispersionRoutine(WeaponState weaponState, float targetDispersionAmount,
														float duration, bool simulateRecoil)
	{
		var startDispersionAmount = simulateRecoil ? MAX_DISPERSION_DEGREE : weaponState.currentDispersionDegrees;
		var elapsed = 0f;
		while (elapsed < duration && duration > 0f)
		{
			elapsed += Time.deltaTime;
			weaponState.currentDispersionDegrees = Mathf.Lerp(startDispersionAmount, targetDispersionAmount, elapsed / duration);
			if (weaponState.isActive)
			{
				OnDispersionChanged?.Invoke(weaponState.currentDispersionDegrees);
			}
			yield return null;
		}
		weaponState.currentDispersionDegrees = targetDispersionAmount;
	}

	private void IncreaseDispersionRate()
	{
		_activeWeaponState.currentDispersionDegrees = Mathf.Min(MAX_DISPERSION_DEGREE, 
									_activeConfig.dispersionIncrementOverTime * _activeWeaponState.currentDispersionDegrees);
		OnDispersionChanged?.Invoke(_activeWeaponState.currentDispersionDegrees);
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.magenta;
			foreach (var pos in _projectileRays)
			{
				var dir = (pos - CachedTransform.position).normalized;
				Gizmos.DrawRay(CachedTransform.position, CachedTransform.position + dir * _activeConfig.range);
			}
		}
	}
}