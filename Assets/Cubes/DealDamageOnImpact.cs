using System;
using UnityEngine;

public class DealDamageOnImpact : MonoBehaviour
{
	public event Action OnImpact;

	public bool destroySelfOnImpact;
	public int damage;
	private bool _hasDealtDamage;

	private void OnEnable()
	{
		_hasDealtDamage = false;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_hasDealtDamage)
		{
			return;
		}

		var collisionManager = ManagerLocator.TryGet<HitManager>();
		if (collisionManager != null)
		{
			collisionManager.ReportHit(other.gameObject, transform.position, damage);
		}

		_hasDealtDamage = true;

		OnImpact?.Invoke();

		if (destroySelfOnImpact)
		{
			Destroy(gameObject);
		}
	}
}
