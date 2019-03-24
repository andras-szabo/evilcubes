using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoWithCachedTransform 
{
	public const int MAX_HIT_PER_SHOT = 256;
	public WeaponConfig config;

	private float _elapsedSecondsSinceLastShot = float.MaxValue;
	private float _currentDispersionDegrees;
	private RaycastHit[] _hits = new RaycastHit[MAX_HIT_PER_SHOT];

	private List<Vector3> _projectileRays = new List<Vector3>();

	private void Awake()
	{
		HandleTriggerLetGo();
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
			//TODO: increase dispersion
			TryShoot();
		}
	}

	private void TryShoot()
	{
		if (_elapsedSecondsSinceLastShot >= config.coolDownSeconds)
		{
			_elapsedSecondsSinceLastShot = 0f;
			_projectileRays.Clear();

			var origin = CachedTransform.position;
			var forward = CachedTransform.forward;
			var up = CachedTransform.up;
			var right = CachedTransform.right;

			for (int i = 0; i < config.projectileCountPerShot; ++i)
			{
				var horizontalDispersion = Random.Range(-_currentDispersionDegrees, _currentDispersionDegrees);
				var verticalDispersion = Random.Range(-_currentDispersionDegrees, _currentDispersionDegrees);

				var hDisplacement = 0f;
				var vDisplacement = 0f;

				if (!Mathf.Approximately(horizontalDispersion, 0f))
				{
					hDisplacement = Mathf.Tan(Mathf.PI / 180f * horizontalDispersion);
				}

				if (!Mathf.Approximately(verticalDispersion, 0f))
				{
					vDisplacement = Mathf.Tan(Mathf.PI / 180f * verticalDispersion);
				}

				var aimPoint = origin + forward + up * vDisplacement + right * hDisplacement;
				var aimDirection = aimPoint - origin;

				_projectileRays.Add(aimPoint);

				var hitCount = Physics.RaycastNonAlloc(origin: origin, direction: aimDirection, results: _hits, maxDistance: config.range);
				if (hitCount > 0)
				{
					ProcessHits();
				}
			}
		}
	}

	private void ProcessHits()
	{
		//TODO
	}

	public void HandleTriggerLetGo()
	{
		_currentDispersionDegrees = config.dispersionDegrees;
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
