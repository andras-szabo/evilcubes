using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoWithCachedTransform
{
	public const int MAX_HIT_PER_SHOT = 256;
	public const float MAX_DISPERSION_DEGREE = 15f;

	public WeaponConfig config;

	private float _elapsedSecondsSinceLastShot = float.MaxValue;
	private float _currentDispersionDegrees;
	private RaycastHit[] _hits = new RaycastHit[MAX_HIT_PER_SHOT];
	private SortedList<float, RaycastHit> _hitsByDistance = new SortedList<float, RaycastHit>(); 

	private List<Vector3> _projectileRays = new List<Vector3>();
	private int _enemyLayerMask;
	private HitManager _hitManager;

	private void Awake()
	{
		_enemyLayerMask = LayerMask.GetMask("EvilCubes");
		HandleTriggerLetGo();
	}

	private void Start()
	{
		_hitManager = ManagerLocator.TryGet<HitManager>();
	}

	private void LateUpdate()
	{
		_elapsedSecondsSinceLastShot += Time.deltaTime;
	}

	public void HandleTriggerPull()
	{
		TryShoot();
	}

	public void HandleTriggerHeld()
	{
		if (config.isAutomatic)
		{
			if (TryShoot())
			{
				IncreaseDispersionRate();
			}
		}
	}

	public void HandleTriggerLetGo()
	{
		ResetDispersionRate();
	}

	private bool TryShoot()
	{
		if (_elapsedSecondsSinceLastShot < config.coolDownSeconds)
		{
			return false;
		}

		_elapsedSecondsSinceLastShot = 0f;
		_projectileRays.Clear();

		var origin = CachedTransform.position;

		for (int i = 0; i < config.projectileCountPerShot; ++i)
		{
			var hDisplacement = 0f;
			var vDisplacement = 0f;

			CalculateBulletDisplacementAtUnitDistance(ref hDisplacement, ref vDisplacement);

			var aimDirection = CachedTransform.forward + CachedTransform.up * vDisplacement +
														 CachedTransform.right * hDisplacement;

			_projectileRays.Add(origin + aimDirection);

			var hitCount = Physics.RaycastNonAlloc(origin: origin, direction: aimDirection, results: _hits, maxDistance: config.range,
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
		var horizontalDispersion = Random.Range(-_currentDispersionDegrees, _currentDispersionDegrees);
		var verticalDispersion = Random.Range(-_currentDispersionDegrees, _currentDispersionDegrees);

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

		var damage = config.damagePerProjectile;

		for (int i = 0; i < _hitsByDistance.Count && damage > 0.1f; ++i)
		{
			var hit = _hitsByDistance.Values[i];
			_hitManager.ReportHit(hit.collider.gameObject, hit.point, damage);
			damage = (int) (damage * (1f - config.dmgReductionRate));
			Debug.Log(damage);
		}

		Debug.Log("---");
	}

	private void ResetDispersionRate()
	{
		_currentDispersionDegrees = config.dispersionDegrees;
	}

	private void IncreaseDispersionRate()
	{
		_currentDispersionDegrees = Mathf.Min(MAX_DISPERSION_DEGREE, config.dispersionIncrementOverTime * _currentDispersionDegrees);
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			Gizmos.color = Color.magenta;
			foreach (var pos in _projectileRays)
			{
				var dir = (pos - CachedTransform.position).normalized;
				Gizmos.DrawRay(CachedTransform.position, CachedTransform.position + dir * config.range);
			}
		}
	}
}
