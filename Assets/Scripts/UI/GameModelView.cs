using UnityEngine;
using UnityEngine.UI;

public class GameModelView : MonoBehaviour
{
	public Text liveEnemyCountLabel;
	public Text deadEnemyCountLabel;
	public Text shotsFiredLabel;
	public Text shotsHitLabel;

	private void Start()
	{
		var gc = ManagerLocator.TryGet<GameController>();

		gc.OnLiveEnemyCountChanged += HandleLiveEnemyCountChanged;
		gc.OnDeadEnemyCountChanged += HandleDeadEnemyCountChanged;
		gc.OnBulletsFiredCountChanged += HandleBulletsFiredCountChanged;
		gc.OnBulletsHitCountChanged += HandleBulletsHitCountChanged;

		HandleLiveEnemyCountChanged(0);
		HandleDeadEnemyCountChanged(0);
		HandleBulletsFiredCountChanged(0);
		HandleBulletsHitCountChanged(0);
	}

	private void OnDestroy()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnLiveEnemyCountChanged -= HandleLiveEnemyCountChanged;
			gc.OnDeadEnemyCountChanged -= HandleDeadEnemyCountChanged;
			gc.OnBulletsFiredCountChanged -= HandleBulletsFiredCountChanged;
		}
	}

	private void HandleBulletsFiredCountChanged(int bulletCount)
	{
		if (shotsFiredLabel != null)
		{
			shotsFiredLabel.text = string.Format("Bullets fired: {0}", bulletCount);
		}
	}

	private void HandleBulletsHitCountChanged(int bulletCount)
	{
		if (shotsHitLabel != null)
		{
			shotsHitLabel.text = string.Format("Bullets hit: {0}", bulletCount);
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
