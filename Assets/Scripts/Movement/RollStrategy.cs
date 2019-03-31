using System.Collections;
using UnityEngine;

public class RollStrategy : AMovementStrategy
{
	private RollMove _roll;
	private float _chanceToRollSideways;

	public RollStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, EnemyConfig config, float speedMultiplier):
				base(cachedTransform, meshToRotate, pathFinder, config.edgeSize, config.speedUnitsPerSecond * speedMultiplier)
	{
		_roll = new RollMove(_cachedTransform, _meshToRotate, _pathFinder, _edgeSize, _rollAnglePerUpdate);
		_chanceToRollSideways = config.sideRollChance;
	}

	public override float GetMaxStepDistance()
	{
		return _edgeSize;
	}

	public override IEnumerator RunRoutine()
	{
		while (true)
		{
			yield return _roll.Execute();
			
			var shouldRollSideWays = Random.Range(0f, 1f) < _chanceToRollSideways;
			if (shouldRollSideWays)
			{
				_roll.RollDirection = Random.Range(0f, 1f) < 0.5f ? RollMove.Direction.Left : RollMove.Direction.Right;
			}
			else
			{
				_roll.RollDirection = RollMove.Direction.Forward;
			}

			StepFinished();
		}
	}
}

