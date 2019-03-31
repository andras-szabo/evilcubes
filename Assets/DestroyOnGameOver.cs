using UnityEngine;

public class DestroyOnGameOver : MonoBehaviour
{
	private void Start()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver += HandleGameOver;
		}
	}

	private void HandleGameOver(bool hasPlayerWon)
	{
		Destroy(this.gameObject);
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
