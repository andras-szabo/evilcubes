using UnityEngine;

public class DestroyOnGameOver : MonoBehaviour
{
	public PoolUser poolUser;

	private void Start()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver += HandleGameOver;
		}
	}

	private void HandleGameOver(GameController.GameResult hasPlayerWon)
	{
		if (poolUser != null)
		{
			poolUser.Despawn();
		}
		else
		{
			Destroy(this.gameObject);
		}
	}

	private void OnDestroy()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver -= HandleGameOver;
		}
	}
}
