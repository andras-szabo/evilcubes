using System.Collections.Generic;
using UnityEngine;

public class HitManager : IManager
{
	private Dictionary<int, IHittable> _collidablesByID = new Dictionary<int, IHittable>();

	public void Cleanup()
	{
		_collidablesByID.Clear();
	}

	public void Register(GameObject gameObject, IHittable hittable)
	{
		if (gameObject != null)
		{
			_collidablesByID.Add(gameObject.GetInstanceID(), hittable);
		}
	}

	public void Unregister(GameObject gameObject)
	{
		if (gameObject != null)
		{
			_collidablesByID.Remove(gameObject.GetInstanceID());
		}
	}

	public void ReportHit(GameObject gameObject, Vector3 hitPosition, int damage)
	{
		IHittable hittable;
		if (_collidablesByID.TryGetValue(gameObject.GetInstanceID(), out hittable))
		{
			hittable.Hit(hitPosition, damage);
		}
	}
}

public interface IHittable
{
	void Hit(Vector3 hitPosition, int damage);
}
