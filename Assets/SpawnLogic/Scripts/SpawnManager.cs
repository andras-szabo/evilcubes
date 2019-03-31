using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public struct EnemyInfo
{
	public EnemyInfo(EnemyType t, int live, int dead)
	{
		affectedEnemyType = t;
		currentLiveEnemyCount = live;
		currentDeadEnemyCount = dead;
	}

	public EnemyType affectedEnemyType;
	public int currentLiveEnemyCount;
	public int currentDeadEnemyCount;
}

public class SpawnManager : MonoBehaviour, IManager
{
	public const float MAX_SPAWN_COUNT_PER_FRAME = 12f;

	public event Action<EnemyInfo> OnEnemyFinishedSpawning;
	public event Action<EnemyInfo> OnEnemyRemoved;

	[HideInInspector]
	public EnemyType[] spawnableEnemies = new EnemyType[]
	{
		EnemyType.Simple,
		EnemyType.Jumper,
		EnemyType.Zigzag
	};

	[HideInInspector]
	public uint spawnID = 0;

	public SpawnConfig config;
	public EnemyConfig[] enemyConfigs;
	public Enemy enemyPrefab;

	public bool startSpawningOnStart;
	private Dictionary<int, Enemy> _spawnedEnemiesByUID = new Dictionary<int, Enemy>();

	private WaveConfig _activeConfig;
	private int _activeWaveIndex;

	private int _liveEnemyCount;
	private int _eliminatedEnemyCount;
	private bool _waitingForTitanSpawnConfirmation;

	private float _elapsedSinceLastSpawn;
	private bool _spawnCancelToken;
	private Coroutine _spawnRoutine;
	private Stack<EnemyType> _failedSpawnTypes = new Stack<EnemyType>();

	private List<EnemyWithCumulativeSpawnChance> _cumulativeSpawnChances = new List<EnemyWithCumulativeSpawnChance>();

	private int _enemyLayerMask;
	private Dictionary<EnemyType, EnemyConfig> _enemyConfigsByType;
	private Vector3[] _candidateSpawnPositions = new Vector3[4];

	private void Awake()
	{
		Init();
		ManagerLocator.TryRegister<SpawnManager>(this);
		ResetSpawningCompletely();
	}

	private void Start()
	{
		if (startSpawningOnStart)
		{
			StartSpawning();
		}
	}

	private void Init()
	{
		_enemyLayerMask = LayerMask.GetMask("EvilCubes");
		_enemyConfigsByType = new Dictionary<EnemyType, EnemyConfig>(spawnableEnemies.Length);

		foreach (var config in enemyConfigs)
		{
			_enemyConfigsByType.Add(config.type, config);
		}
	}

	public void StartSpawning()
	{
		_spawnRoutine = StartCoroutine(SpawnRoutine());
	}

	public void StopSpawning()
	{
		if (_spawnRoutine != null)
		{
			StopCoroutine(_spawnRoutine);
		}

		_spawnRoutine = null;
	}

	private void SetActiveConfig(int waveConfigIndex)
	{
		_activeWaveIndex = waveConfigIndex;
		_activeConfig = config.waves[_activeWaveIndex];

		_cumulativeSpawnChances.Clear();
		var totalSpawnChance = 0f;

		foreach (var enemyType in spawnableEnemies)
		{
			var spawnChance = _activeConfig.GetSpawnChance(enemyType);
			if (spawnChance > 0f)
			{
				totalSpawnChance += spawnChance;
				_cumulativeSpawnChances.Add(new EnemyWithCumulativeSpawnChance(totalSpawnChance, enemyType));
			}
		}
	}

	private IEnumerator SpawnRoutine()
	{
		Reset();
		SetActiveConfig(_activeWaveIndex);

		var spawnInterval = Mathf.Max(WaveConfig.MIN_SPAWN_INTERVAL, _activeConfig.spawnIntervalSeconds);

		while (!_spawnCancelToken)
		{
			_elapsedSinceLastSpawn += Time.deltaTime;
			var idealSpawnCount = Mathf.Min(_elapsedSinceLastSpawn / spawnInterval, MAX_SPAWN_COUNT_PER_FRAME);
			if (idealSpawnCount >= 1f)
			{
				for (int i = 0; i < (int)idealSpawnCount; ++i)
				{
					if (_spawnedEnemiesByUID.Count < _activeConfig.maxLiveCubeCount && !_waitingForTitanSpawnConfirmation)
					{
						if (TrySpawnNewEnemy(_activeConfig, _cumulativeSpawnChances))
						{
							_elapsedSinceLastSpawn -= spawnInterval;
						}
					}
				}
			}

			yield return null;
		}

		_spawnRoutine = null;
	}

	public void Cleanup()
	{
		foreach (var spawnedEnemy in _spawnedEnemiesByUID.Values)
		{
			Destroy(spawnedEnemy.gameObject);
		}

		_spawnedEnemiesByUID.Clear();
		_liveEnemyCount = 0;
		spawnID = 0;
	}

	public bool TrySpawnNewEnemy(WaveConfig config, List<EnemyWithCumulativeSpawnChance> enemiesByCumulativeSpawnChance)
	{
		var success = true;
		EnemyType enemyToSpawnType;

		if (_eliminatedEnemyCount >= _activeConfig.titanToAppearAt)
		{
			enemyToSpawnType = EnemyType.Titan;
		}
		else
		{
			if (_failedSpawnTypes.Count > 0)
			{
				enemyToSpawnType = _failedSpawnTypes.Pop();
			}
			else
			{
				enemyToSpawnType = PickRandomEnemyToSpawn(enemiesByCumulativeSpawnChance);
			}
		}

		EnemyConfig enemyConfig;
		Vector3 spawnPosition;

		success &= _enemyConfigsByType.TryGetValue(enemyToSpawnType, out enemyConfig);
		success &= TryPickRandomPositionToSpawn(enemyConfig, out spawnPosition);

		if (success) { DoSpawn(enemyConfig, spawnPosition, config.cubeSpeedMultiplier); }
		else { _failedSpawnTypes.Push(enemyConfig.type); }

		if (success && enemyToSpawnType == EnemyType.Titan)
		{
			_waitingForTitanSpawnConfirmation = true;
		}

		return success;
	}

	private void DoSpawn(EnemyConfig enemyConfig, Vector3 spawnPosition, float enemySpeedMultiplier)
	{
		var rotationToFacePlayer = Quaternion.LookRotation(new Vector3(-spawnPosition.x, 0f, -spawnPosition.z), Vector3.up);
		var nme = Instantiate<Enemy>(enemyPrefab, spawnPosition, rotationToFacePlayer);

		nme.gameObject.name = string.Format("Spawnee {0} {1}", spawnID++, enemyConfig.type);
		nme.Setup(enemyConfig, enemySpeedMultiplier);

		_spawnedEnemiesByUID.Add(nme.gameObject.GetInstanceID(), nme);

		nme.OnFinishedSpawning += HandleEnemyFinishedSpawning;
		nme.OnRemoved += HandleEnemyRemoved;
	}

	private void HandleEnemyFinishedSpawning(Enemy enemy)
	{
		_liveEnemyCount++;
		OnEnemyFinishedSpawning?.Invoke(new EnemyInfo(enemy.Type, _liveEnemyCount, _eliminatedEnemyCount));

		if (enemy.Type == EnemyType.Titan)
		{
			_waitingForTitanSpawnConfirmation = false;
			_spawnCancelToken = true;
		}
	}

	private void HandleEnemyRemoved(Enemy enemy)
	{
		enemy.OnRemoved -= HandleEnemyRemoved;
		_spawnedEnemiesByUID.Remove(enemy.gameObject.GetInstanceID());

		if (!enemy.IsSpawning)
		{
			_liveEnemyCount--;
			_eliminatedEnemyCount++;

			OnEnemyRemoved?.Invoke(new EnemyInfo(enemy.Type, _liveEnemyCount, _eliminatedEnemyCount));
		}
		else
		{
			if (enemy.Type == EnemyType.Titan)
			{
				// We have unsuccessfully tried to spawn a titan. We have to try again.
				_waitingForTitanSpawnConfirmation = false;
			}
		}
	}

	private bool TryPickRandomPositionToSpawn(EnemyConfig enemyConfig, out Vector3 position)
	{
		var triesLeft = 8;

		while (enemyConfig != null && triesLeft-- > 0)
		{
			// This is assuming the player is in the origin.

			var distance = UnityEngine.Random.Range(_activeConfig.minSpawnDistanceFromPlayer, _activeConfig.maxSpawnDistanceFromPlayer);
			var angle = UnityEngine.Random.Range(0f, 360f) * Mathf.PI / 180f;
			var sin = Mathf.Sin(angle) * distance;
			var cos = Mathf.Cos(angle) * distance;

			var centreHeight = enemyConfig.edgeSize / 2f;

			_candidateSpawnPositions[0] = new Vector3(cos, centreHeight, sin);
			_candidateSpawnPositions[1] = new Vector3(sin, centreHeight, -cos);
			_candidateSpawnPositions[2] = new Vector3(-cos, centreHeight, -sin);
			_candidateSpawnPositions[3] = new Vector3(-sin, centreHeight, cos);

			foreach (var candidatePosition in _candidateSpawnPositions)
			{
				if (CanSpawnEnemyAt(enemyConfig, candidatePosition))
				{
					position = candidatePosition;
					return true;
				}
			}
		}

		position = Vector3.zero;

		return false;
	}

	private Collider[] overlappingColliders = new Collider[128];

	private bool CanSpawnEnemyAt(EnemyConfig config, Vector3 position)
	{
		var overlapCount = Physics.OverlapSphereNonAlloc(position, config.halfBodyDiagonal + 0.1f, overlappingColliders, _enemyLayerMask);
		return overlapCount < 1;
	}

	public static EnemyType PickRandomEnemyToSpawn(List<EnemyWithCumulativeSpawnChance> enemiesByCumulativeSpawnChance,
												   float chanceOverride = -1f)
	{
		if (enemiesByCumulativeSpawnChance == null) { return EnemyType.Simple; }
		var spawnableEnemyCount = enemiesByCumulativeSpawnChance.Count;
		if (spawnableEnemyCount < 1)
		{
			return (EnemyType)UnityEngine.Random.Range((int)EnemyType.Simple, (int)EnemyType.Zigzag + 1);
		}

		float diceRoll = chanceOverride;

		if (chanceOverride < 0f)
		{
			diceRoll = UnityEngine.Random.Range(0f, enemiesByCumulativeSpawnChance[spawnableEnemyCount - 1].spawnChance);
		}

		foreach (var enemy in enemiesByCumulativeSpawnChance)
		{
			if (enemy.spawnChance >= diceRoll)
			{
				return enemy.type;
			}
		}

		return EnemyType.Simple;
	}

	public void Reset()
	{
		_activeConfig = null;
		_activeWaveIndex = 0;
		_eliminatedEnemyCount = 0;
		_elapsedSinceLastSpawn = 0f;
		_liveEnemyCount = 0;
		_spawnCancelToken = false;
		_cumulativeSpawnChances.Clear();
	}

	public void ResetSpawningCompletely()
	{
		StopSpawning();
		Reset();
	}
}

public struct EnemyWithCumulativeSpawnChance
{
	public readonly float spawnChance;
	public readonly EnemyType type;

	public EnemyWithCumulativeSpawnChance(float chance, EnemyType t)
	{
		spawnChance = chance;
		type = t;
	}
}
