using UnityEngine;

public struct HPInfo
{
	public int current;
	public int previous;
	public int max;

	public float RateToFull { get { return (float)current / max; } }
}

public class HP : MonoBehaviour, IHittable
{
	public int hitPoints = 100;
	public bool destroyWhenHPzero;
	private int _startingHP;
	private int _previousHP;
	public bool log;

	public event System.Action<HPInfo> OnHitPointsChanged;

	public HPInfo GetHPInfo()
	{
		return new HPInfo { current = hitPoints, max = _startingHP, previous = _previousHP };
	}

	private void Awake()
	{
		_startingHP = hitPoints;
		_previousHP = hitPoints;
	}

	public void Reset()
	{
		_previousHP = hitPoints;
		hitPoints = _startingHP;
		OnHitPointsChanged?.Invoke(GetHPInfo());
	}

	public void SetStartingHP(int hp)
	{
		hitPoints = hp;
		_startingHP = hp;
		_previousHP = hp;
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
			_previousHP = hitPoints;
			hitPoints = System.Math.Max(0, hitPoints - damage);
			OnHitPointsChanged?.Invoke(new HPInfo { current = hitPoints, max = _startingHP, previous = _previousHP });
			if (hitPoints <= 0 && destroyWhenHPzero)
			{
				Destroy(gameObject);
			}
		}
	}
}
