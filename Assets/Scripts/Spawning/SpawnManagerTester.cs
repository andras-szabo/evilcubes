using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManagerTester : MonoBehaviour
{
	[Range(0, 1000)] public int seed;
	[Range(1f, 100f)] public float testUnitDuration;

	public IEnumerator Start()
	{
		var spawnManager = ManagerLocator.TryGet<SpawnManager>();

		while (spawnManager == null)
		{
			yield return null;
			spawnManager = ManagerLocator.TryGet<SpawnManager>();
		}

		while (true)
		{
			Debug.Log("Starting to spawn with seed " + (++seed));
			Random.InitState(seed);	

			spawnManager.StartSpawning();

			var totalElapsed = 0f;
			while (totalElapsed < testUnitDuration)
			{
				totalElapsed += Time.deltaTime;
				yield return null;
			}

			spawnManager.StopSpawning();
			spawnManager.RemoveSpawnedEnemies();
			spawnManager.Reset();
			
			yield return null;
		}
	}
}
