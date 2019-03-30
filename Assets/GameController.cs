using UnityEngine;

public class GameController : MonoBehaviour, IManager
{
	public GameModel GameModel { get; private set; }

	private SpawnManager _spawner;

	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;

		ManagerLocator.TryRegister<GameController>(this);
		GameModel = new GameModel();
	}

	private void Start()
	{
		CacheDependencies();
		SetupObservers();
	}

	private void OnDestroy()
	{
		ManagerLocator.Cleanup();
	}

	private void CacheDependencies()
	{
		_spawner = ManagerLocator.TryGet<SpawnManager>();
	}

	private void SetupObservers()
	{
		_spawner.OnEnemyFinishedSpawning += GameModel.HandleEnemySpawned;
		_spawner.OnEnemyRemoved += GameModel.HandleEnemyRemoved;
	}

	private void CleanupObservers()
	{
		_spawner.OnEnemyFinishedSpawning -= GameModel.HandleEnemySpawned;
		_spawner.OnEnemyRemoved -= GameModel.HandleEnemyRemoved;

	}
}
