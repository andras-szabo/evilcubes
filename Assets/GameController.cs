using System;
using UnityEngine;

public class GameController : MonoBehaviour, IManager
{
	public event Action<int> OnLiveEnemyCountChanged;
	public event Action<int> OnDeadEnemyCountChanged;
	public event Action<int> OnBulletsFiredCountChanged;
	public event Action<int> OnBulletsHitCountChanged;

	public event Action<bool> OnGameOver;

	private GameModel _gameModel;

	private SpawnManager _spawner;
	private PlayerController _player;
	private HUD _hud;

	public bool IsPlaying
	{
		get
		{
			return _gameModel != null && _gameModel.isGameOngoing;
		}
	}

	private void Awake()
	{
		ManagerLocator.TryRegister<GameController>(this);
	}

	private void Start()
	{
		CacheDependencies();
		SetupObservers();

		_player.enabled = false;
		_hud.ShowHUD(false);
	}

	private void OnDestroy()
	{
		ManagerLocator.Cleanup();
	}

	public void ShowCursor(bool state)
	{
		if (state)
		{
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
		}
		else
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	public void StartNewGame()
	{
		_spawner.StopSpawning();
		_spawner.Cleanup();
		_spawner.Reset();

		_gameModel = new GameModel();
		_gameModel.isGameOngoing = true;

		OnBulletsFiredCountChanged?.Invoke(0);
		OnBulletsHitCountChanged?.Invoke(0);
		OnLiveEnemyCountChanged?.Invoke(0);
		OnDeadEnemyCountChanged?.Invoke(0);

		_player.enabled = true;
		_player.Setup();
		ShowCursor(false);
		_hud.ShowHUD(true);

		_spawner.StartSpawning();

		Time.timeScale = 1f;
	}

	public void PauseGame(bool pause)
	{
		if (_gameModel != null && _gameModel.isGameOngoing)
		{
			Time.timeScale = pause ? 0f : 1f;
			ShowCursor(pause);
			_player.Paused = pause;
		}
	}

	public void AbortGame(bool hasPlayerWon)
	{
		_spawner.StopSpawning();
		_spawner.Cleanup();
		_spawner.Reset();

		_gameModel.isGameOngoing = false;
		_gameModel.hasPlayerWon = hasPlayerWon;
		_gameModel.isGameOver = true;

		_player.enabled = false;
		_hud.ShowHUD(false);
	}

	public void Teardown()
	{
		_gameModel = null;

		CleanupObservers();
		OnBulletsFiredCountChanged = null;
		OnBulletsHitCountChanged = null;
		OnLiveEnemyCountChanged = null;
		OnDeadEnemyCountChanged = null;

		ManagerLocator.Cleanup();
	}

	private void CacheDependencies()
	{
		_spawner = ManagerLocator.TryGet<SpawnManager>();
		_player = ManagerLocator.TryGet<PlayerController>();
		_hud = ManagerLocator.TryGet<HUD>();
	}

	private void SetupObservers()
	{
		_spawner.OnEnemyFinishedSpawning += HandleEnemySpawned;
		_spawner.OnEnemyRemoved += HandleEnemyRemoved;

		_player.weaponController.OnShotFired += HandleShotFired;
		_player.OnPlayerDead += HandlePlayerDead;
	}

	private void CleanupObservers()
	{
		_spawner.OnEnemyFinishedSpawning -= HandleEnemySpawned;
		_spawner.OnEnemyRemoved -= HandleEnemyRemoved;

		_player.weaponController.OnShotFired -= HandleShotFired;
		_player.OnPlayerDead -= HandlePlayerDead;
	}

	private void HandlePlayerDead()
	{
		OnGameOver?.Invoke(false);
	}

	public void HandleEnemySpawned(EnemyInfo info)
	{
		UpdateStats(info);
		OnLiveEnemyCountChanged?.Invoke(_gameModel.liveEnemyCount);
	}

	public void HandleEnemyRemoved(EnemyInfo info)
	{
		UpdateStats(info);
		OnLiveEnemyCountChanged?.Invoke(_gameModel.liveEnemyCount);
		OnDeadEnemyCountChanged?.Invoke(_gameModel.deadEnemyCount);

		if (info.affectedEnemyType == EnemyType.Titan)
		{
			OnGameOver?.Invoke(true);
		}
	}

	public void HandleShotFired(ShotInfo info)
	{
		_gameModel.bulletsFiredCount += info.bulletsFired;
		_gameModel.bulletsHitcount += info.bulletsHit;

		OnBulletsFiredCountChanged?.Invoke(_gameModel.bulletsFiredCount);
		if (info.bulletsHit > 0)
		{
			OnBulletsHitCountChanged?.Invoke(_gameModel.bulletsHitcount);
		}
	}

	private void UpdateStats(EnemyInfo info)
	{
		_gameModel.liveEnemyCount = info.currentLiveEnemyCount;
		_gameModel.deadEnemyCount = info.currentDeadEnemyCount;
	}
}
