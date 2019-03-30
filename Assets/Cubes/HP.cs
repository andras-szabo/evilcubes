using UnityEngine;

public struct HPInfo
{
	public int current;
	public int max;

	public float RateToFull { get { return (float)current / max; } }
}

public class HP : MonoBehaviour, IHittable
{
	public int hitPoints = 100;
	public bool destroyWhenHPzero;
	private int _startingHP;
	public bool log;

	public event System.Action<HPInfo> OnHitPointsChanged;

	public HPInfo GetHPInfo()
	{
		return new HPInfo { current = hitPoints, max = _startingHP };
	}

	private void Awake()
	{
		_startingHP = hitPoints;
	}

	private void Start()
	{
		var hitManager = ManagerLocator.TryGet<HitManager>();
		if (hitManager != null)
		{
			hitManager.Register(gameObject, this);
			if (log)
			{
				Debug.Log("Registering: " + gameObject.name);
			}
		}
	}

	private void OnDestroy()
	{
		var hitManager = ManagerLocator.TryGet<HitManager>();
		if (hitManager != null)
		{
			hitManager.Unregister(gameObject);
		}
	}

	public void Hit(Vector3 position, int damage)
	{
		if (hitPoints >= 0 && damage > 0)
		{
			hitPoints = System.Math.Max(0, hitPoints - damage);
			OnHitPointsChanged?.Invoke(new HPInfo { current = hitPoints, max = _startingHP });
			if (hitPoints <= 0 && destroyWhenHPzero)
			{
				Destroy(gameObject);
			}
		}
	}
}
