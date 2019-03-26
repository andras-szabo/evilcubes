using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AMovementStrategy
{
	public abstract IEnumerator RunRoutine();
}

public class JumpStrategy : AMovementStrategy
{
	private JumpMove _jump;
	private RollMove _roll;
	private Transform _cachedTransform;
	private float _jumpDistance;
	private float _jumpChance;

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
		}
	}

	private bool ShouldJump()
	{
		return IsPlayerOutOfJumpDistance() && HasRandomChanceToJump();
	}

	private bool IsPlayerOutOfJumpDistance()
	{
		return Vector3.SqrMagnitude(-_cachedTransform.position) > _jumpDistance;
	}

	private bool HasRandomChanceToJump()
	{
		return Random.Range(0f, 1f) < _jumpChance;
	}
}

public class RollStrategy : AMovementStrategy
{
	public override IEnumerator RunRoutine()
	{
		while (true)
		{
			if (ShouldRollSideWays())
			{
				yield return _rollSideways.Execute();
			}
			else
			{
				yield return _rollForward.Execute();
			}
		}
	}
}

public abstract class AMove
{
	protected PathFinder _pathFinder;
	protected float _halfBodyDiagonal;

	private WaitForSeconds _pathCheckInterval = new WaitForSeconds(0.1f);

	protected IEnumerator WaitUntilPathFreeRoutine(IEnumerable<Vector3> path)
	{
		while (!_pathFinder.IsPathFree(path, _halfBodyDiagonal))
		{
			yield return _pathCheckInterval;
		}
	}
}

public class JumpMove : AMove
{
	private float _jumpForce;
	private float _jumpAngle;

	private float _jumpDuration;
	private Vector3 _initialVelocity;
	private Transform _cachedTransform;
	private Transform _meshToRotate;
	private bool _jumpForward;
	private List<Vector3> _trajectory = new List<Vector3>();

	private const int _trajectorySectionCount = 16;
	private int _previousRemainingSectionCount;

	public IEnumerator Execute()
	{
		CalculateTrajectory(_jumpForce, _jumpAngle);
		var plannedPath = _trajectory.GetRange(1, _trajectory.Count - 1);
		yield return WaitUntilPathFreeRoutine(plannedPath);
		_pathFinder.UpdatePath(plannedPath);

		var elapsedTime = 0f;
		var startingPoint = _cachedTransform.position;
		var endPoint = _cachedTransform.position + new Vector3(_initialVelocity.x * _jumpDuration, 0f, _initialVelocity.z * _jumpDuration);
		_previousRemainingSectionCount = 0;	

		while (elapsedTime < _jumpDuration)
		{
			elapsedTime += Time.deltaTime;

			var dx = _initialVelocity.x * elapsedTime;
			var dy = _initialVelocity.y * elapsedTime + Physics.gravity.y * elapsedTime * elapsedTime / 2f;
			var dz = _initialVelocity.z * elapsedTime;

			_cachedTransform.position = startingPoint + new Vector3(dx, dy, dz);
			_meshToRotate.localRotation = Quaternion.Euler(90f * (_jumpForward ? 1f : -1f) * (elapsedTime / _jumpDuration), 0f, 0f);
			UpdatePath(elapsedTime);
			yield return null;
		}

		_cachedTransform.position = endPoint;
		_pathFinder.ClearPath();
	}

	private void CalculateTrajectory(float jForce, float jumpAngle)
	{
		var g = Physics.gravity.y;

		jumpAngle = _jumpForward ? -jumpAngle : 180f + jumpAngle;

		var rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(jumpAngle, _cachedTransform.right));
		_initialVelocity = (rotation.MultiplyVector(_cachedTransform.forward)).normalized * jForce;

		_jumpDuration = -2f * _initialVelocity.y / g;

		_trajectory = new List<Vector3>();

		for (int i = 1; i <= _trajectorySectionCount; ++i)
		{
			var t = (_jumpDuration / _trajectorySectionCount) * i;

			var dx = _initialVelocity.x * t;
			var dy = _initialVelocity.y * t + g * t * t / 2f;
			var dz = _initialVelocity.z * t;

			_trajectory.Add(_cachedTransform.position + new Vector3(dx, dy, dz));
		}
	}

	private void UpdatePath(float elapsedTime)
	{
		var elapsedTrajectorySections = (int)(elapsedTime / (_jumpDuration / _trajectorySectionCount));
		
		if (elapsedTrajectorySections < 1)
		{
			elapsedTrajectorySections = 1;
		}

		var remainingSectionCount = _trajectorySectionCount - elapsedTrajectorySections;
		if (_previousRemainingSectionCount != remainingSectionCount)
		{
			_previousRemainingSectionCount = remainingSectionCount;
			_pathFinder.UpdatePath(_trajectory.GetRange(elapsedTrajectorySections, remainingSectionCount));
		}
	}
}

public class RollMove
{
}
