using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

public class SpawnLogicTests
{
    [Test]
    public void SimpleSpawnLogicTest()
	{
		var chances = new List<EnemyWithCumulativeSpawnChance>();
		chances.Add(new EnemyWithCumulativeSpawnChance(1f, EnemyType.Zigzag));
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances) == EnemyType.Zigzag);

		chances.Add(new EnemyWithCumulativeSpawnChance(2f, EnemyType.Jumper));
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances, 0.9f) == EnemyType.Zigzag);
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances, 1.2f) == EnemyType.Jumper);

		chances.Add(new EnemyWithCumulativeSpawnChance(2.25f, EnemyType.Simple));
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances, 0.12f) == EnemyType.Zigzag);
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances, 1.99f) == EnemyType.Jumper);
		Assert.IsTrue(SpawnManager.PickRandomEnemyToSpawn(chances, 2.01f) == EnemyType.Simple);
    }
}
