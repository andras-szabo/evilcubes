using System.Collections;
using UnityEngine;

public class JumpStrategy : AMovementStrategy
{
	private JumpMove _jump;
	private RollMove _roll;

	private float _jumpChance;
	private float _maxJumpDistance;

	public JumpStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, EnemyConfig config, float speedMultiplier):
		base(cachedTransform, meshToRotate, pathFinder, config.edgeSize, config.speedUnitsPerSecond * speedMultiplier)
	{
		_jump = new JumpMove(_cachedTransform, _meshToRotate, _pathFinder, config.jumpForce, config.jumpAngle);
		_roll = new RollMove(_cachedTransform, _meshToRotate, _pathFinder, _edgeSize, _rollAnglePerUpdate);

		_maxJumpDistance = _jump.CalculateMaxJumpDistance();
		_jumpChance = config.jumpChance;
	}

	public override float GetMaxStepDistance()
	{
		return _maxJumpDistance;
	}

	public override IEnumerator RunRoutine()
	{
		while (true)
		{
			if (ShouldJump())
			{
				yield return _jump.Execute();
			}
			else
			{
				yield return _roll.Execute();
			}

			StepFinished();
		}
	}

	private bool ShouldJump()
	{
		return IsPlayerOutOfJumpDistance() && HasRandomChanceToJump();
	}

	private bool IsPlayerOutOfJumpDistance()
	{
		return Vector3.SqrMagnitude(-_cachedTransform.position) > (_maxJumpDistance * _maxJumpDistance);
	}

	private bool HasRandomChanceToJump()
	{
		return UnityEngine.Random.Range(0f, 1f) < _jumpChance;
	}
}
