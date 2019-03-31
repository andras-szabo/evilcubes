using UnityEngine;

public class PoolUser : MonoBehaviour 
{
	private PoolManager _poolManager;

	private void Start()
	{
		_poolManager = ManagerLocator.TryGet<PoolManager>();			
	}

	public void Despawn()
	{
		if (_poolManager != null)
		{
			_poolManager.Despawn(this);
		}
		else
		{
			Destroy(gameObject);
		}
	}
}
