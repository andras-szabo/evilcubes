using UnityEngine;

[CreateAssetMenu()]
public class WaveConfig : ScriptableObject
{
	public const float MIN_SPAWN_INTERVAL = 0.01f;

	public string id;

	[Header("Spawn timing")]
	[Range(MIN_SPAWN_INTERVAL, 2f)]	public float spawnIntervalSeconds;
	[Range(1, 3000)] public float maxLiveCubeCount;

	[Header("Spawn chances")]
	[Range(0f, 1f)] public float spawnChanceSimple;
	[Range(0f, 1f)] public float spawnChanceJumper;
	[Range(0f, 1f)] public float spawnChanceZigZag;
	
	[Tooltip("Titan to appear after eliminating this many cubes.")]
	[Range(1, 500)] public int titanToAppearAt;

	[Header("Spawn distance &c")]
	[Range(10f, 1000f)] public float minSpawnDistanceFromPlayer;
	[Range(10f, 1000f)] public float maxSpawnDistanceFromPlayer;
	[Range(0.2f, 100f)] public float cubeSpeedUnitsPerSec;

	private void OnValidate()
	{
		maxSpawnDistanceFromPlayer = Mathf.Max(minSpawnDistanceFromPlayer, maxSpawnDistanceFromPlayer);
	}

	public float GetSpawnChance(EnemyType type)
	{
		switch (type)
		{
			case EnemyType.Simple: return spawnChanceSimple;
			case EnemyType.Jumper: return spawnChanceJumper;
			case EnemyType.Zigzag: return spawnChanceZigZag;
		}

		return 0f;
	}
}
