using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ShotInfo
{
	public ShotInfo(int fired, int hit)
	{
		bulletsFired = fired;
		bulletsHit = hit;
	}

	public readonly int bulletsFired;
	public readonly int bulletsHit;
}

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
			defaultDispersionDegrees = config.dispersionDegrees;
		}

		readonly public string name;
		readonly public float cooldown;
		readonly public float defaultDispersionDegrees;

		public bool isActive;
		public float elapsedSinceLastShot;
		public float currentDispersionDegrees;
	}

	public const int MAX_HIT_PER_SHOT = 256;
	public const float MAX_DISPERSION_DEGREE = 25f;

	public event Action<WeaponState> OnWeaponChanged;
	public event Action<float> OnDispersionChanged;
	public event Action<ShotInfo> OnShotFired;

	public Transform barrel;

	public WeaponConfig[] configs;
	public int WeaponCount
	{
		get
		{
			return configs.Length;
		}
	}

	private WeaponConfig _activeConfig;
	private int _activeConfigIndex;
	private WeaponState _activeWeaponState;
	private WeaponState[] _weaponStates;

	private RaycastHit[] _hits = new RaycastHit[MAX_HIT_PER_SHOT];
	private SortedList<float, RaycastHit> _hitsByDistance = new SortedList<float, RaycastHit>(); 

	private List<Vector3> _projectileRays = new List<Vector3>();
	private int _enemyLayerMask;
	private HitManager _hitManager;
	private PoolManager _pool;
	private bool _isActive;

	public WeaponState CurrentWeaponState
	{
		get
		{
			return _activeWeaponState;
		}
	}

	private void Awake()
	{
		_enemyLayerMask = LayerMask.GetMask("EvilCubeBodyParts");
		
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
		_pool = ManagerLocator.TryGet<PoolManager>();

		ManagerLocator.TryGet<GameController>().OnGameOver += HandleGameOver;
		ManagerLocator.TryGet<GameController>().OnGameStart += HandleGameStart;
	}
	
	private void LateUpdate()
	{
		if (_isActive)
		{
			foreach (var weaponState in _weaponStates)
			{
				weaponState.elapsedSinceLastShot += Time.deltaTime;
			}
		}
	}

	private void OnDestroy()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver -= HandleGameOver;
			gc.OnGameStart -= HandleGameStart;
		}
	}

	private void HandleGameStart()
	{
		_isActive = true;
		ResetWeaponStates();
	}

	private void HandleGameOver(GameController.GameResult hasPlayerWon)
	{
		_isActive = false;
	}

	public void ResetWeaponStates()
	{
		foreach (var weaponState in _weaponStates)
		{
			weaponState.elapsedSinceLastShot = weaponState.cooldown;
			weaponState.currentDispersionDegrees = weaponState.defaultDispersionDegrees;

			if (weaponState == _activeWeaponState)
			{
				OnDispersionChanged?.Invoke(weaponState.currentDispersionDegrees);
			}
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
		int bulletsFired = 0;
		int bulletsHit = 0;

		if (TryShoot(out bulletsFired, out bulletsHit))
		{
			OnShotFired?.Invoke(new ShotInfo(bulletsFired, bulletsHit));

			if (_activeConfig.isAutomatic)
			{
				IncreaseDispersionRate();
			}
			else
			{
				StartCoroutine(SimulateRecoilDispersionRoutine(_activeWeaponState, simulateRecoil: true));
			}

			ShowBulletTrails();
		}
	}

	public void HandleTriggerHeld()
	{
		if (_activeConfig.isAutomatic)
		{
			int bulletsFired = 0;
			int bulletsHit = 0;
			if (TryShoot(out bulletsFired, out bulletsHit))
			{
				OnShotFired?.Invoke(new ShotInfo(bulletsFired, bulletsHit));
				IncreaseDispersionRate();
				ShowBulletTrails();
			}
		}
	}

	public void HandleTriggerLetGo()
	{
		if (_activeConfig.isAutomatic)
		{
			StartCoroutine(SimulateRecoilDispersionRoutine(_activeWeaponState, false));
		}
	}

	private bool TryShoot(out int bulletsFired, out int bulletsHit)
	{
		bulletsFired = 0; bulletsHit = 0;

		if (_activeWeaponState.elapsedSinceLastShot < _activeConfig.coolDownSeconds)
		{
			return false;
		}

		bulletsFired = _activeConfig.projectileCountPerShot;

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
				bulletsHit += 1;
			}
		}
		return true;
	}

	private void CalculateBulletDisplacementAtUnitDistance(ref float hDisplacement, ref float vDisplacement)
	{
		var currentDispersionDegrees = _activeWeaponState.currentDispersionDegrees;
		var horizontalDispersion = UnityEngine.Random.Range(-currentDispersionDegrees, currentDispersionDegrees);
		var verticalDispersion = UnityEngine.Random.Range(-currentDispersionDegrees, currentDispersionDegrees);

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

	private IEnumerator SimulateRecoilDispersionRoutine(WeaponState weaponState, bool simulateRecoil)
	{
		var startDispersionAmount = simulateRecoil ? MAX_DISPERSION_DEGREE : weaponState.currentDispersionDegrees;
		var elapsed = 0f;
		while (_isActive && elapsed < weaponState.cooldown && weaponState.cooldown > 0f)
		{
			elapsed += Time.deltaTime;
			weaponState.currentDispersionDegrees = Mathf.Lerp(startDispersionAmount, weaponState.defaultDispersionDegrees, 
															  elapsed / weaponState.cooldown);
			if (weaponState.isActive)
			{
				OnDispersionChanged?.Invoke(weaponState.currentDispersionDegrees);
			}
			yield return null;
		}
		weaponState.currentDispersionDegrees = weaponState.defaultDispersionDegrees;
	}

	private void IncreaseDispersionRate()
	{
		_activeWeaponState.currentDispersionDegrees = Mathf.Min(MAX_DISPERSION_DEGREE, 
									_activeConfig.dispersionIncrementOverTime * _activeWeaponState.currentDispersionDegrees);
		OnDispersionChanged?.Invoke(_activeWeaponState.currentDispersionDegrees);
	}
	
	private void ShowBulletTrails()
	{
		foreach (var ray in _projectileRays)
		{
			var trail = _pool.Spawn<BulletTrail>(_activeConfig.bulletTrail, barrel.position, barrel.rotation, null);
			var dir = (ray - CachedTransform.position).normalized;
			trail.Setup(CachedTransform.position + dir * 8f);
		}
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
