using System;

public class GameModel
{
	public event Action<int> OnLiveEnemyCountChanged;
	public event Action<int> OnDeadEnemyCountChanged;
	public event Action<int> OnBulletsFiredCountChanged;
	public event Action<int> OnBulletsHitCountChanged;

	public int LiveEnemyCount { get; private set; }
	public int DeadEnemyCount { get; private set; }
	public int BulletsFiredCount { get; private set; }
	public int BulletsHitCount { get; private set; }

	public void HandleEnemySpawned(EnemyInfo info)
	{
		UpdateStats(info);
		OnLiveEnemyCountChanged?.Invoke(LiveEnemyCount);
	}

	public void HandleEnemyRemoved(EnemyInfo info)
	{
		UpdateStats(info);
		OnLiveEnemyCountChanged?.Invoke(LiveEnemyCount);
		OnDeadEnemyCountChanged?.Invoke(DeadEnemyCount);
	}

	private void UpdateStats(EnemyInfo info)
	{
		LiveEnemyCount = info.currentLiveEnemyCount;
		DeadEnemyCount = info.currentDeadEnemyCount;
	}
}
