using UnityEngine;
using UnityEngine.UI;

public class GameModelView : MonoBehaviour
{
	public Text liveEnemyCountLabel;
	public Text deadEnemyCountLabel;

	private void Start()
	{
		var gameModel = ManagerLocator.TryGet<GameController>().GameModel;

		gameModel.OnLiveEnemyCountChanged += HandleLiveEnemyCountChanged;
		gameModel.OnDeadEnemyCountChanged += HandleDeadEnemyCountChanged;

		HandleLiveEnemyCountChanged(0);
		HandleDeadEnemyCountChanged(0);
	}

	private void OnDestroy()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null && gc.GameModel != null)
		{
			gc.GameModel.OnLiveEnemyCountChanged -= HandleLiveEnemyCountChanged;
			gc.GameModel.OnDeadEnemyCountChanged -= HandleDeadEnemyCountChanged;
		}
	}

	private void HandleLiveEnemyCountChanged(int liveEnemyCount)
	{
		if (liveEnemyCountLabel != null)
		{
			liveEnemyCountLabel.text = string.Format("Live enemies: {0}", liveEnemyCount);
		}
	}

	private void HandleDeadEnemyCountChanged(int deadEnemyCount)
	{
		if (deadEnemyCountLabel != null)
		{
			deadEnemyCountLabel.text = string.Format("Dead enemies: {0}", deadEnemyCount);
		}
	}
}
