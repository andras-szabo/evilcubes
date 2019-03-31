using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpMove : AMove
{
	private float _jumpForce;
	private float _jumpAngle;
	private Transform _cachedTransform;
	private Transform _meshToRotate;

	private bool _jumpForward;

	private float _jumpDuration;
	private Vector3 _initialVelocity;
	private const int _trajectorySectionCount = 16;
	private List<Vector3> _trajectory;

	private int _previousRemainingSectionCount;

	public JumpMove(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, float jumpForce, float jumpAngle)
	{
		_jumpForce = jumpForce;
		_jumpAngle = jumpAngle;
		_jumpForward = true;

		_cachedTransform = cachedTransform;
		_meshToRotate = meshToRotate;
		_pathFinder = pathFinder;

		_trajectory = new List<Vector3>(_trajectorySectionCount);
	}

	public IEnumerator Execute()
	{
		CalculateTrajectory(_jumpForce, _jumpAngle);
		var plannedPath = _trajectory.GetRange(1, _trajectory.Count - 1);
		yield return WaitUntilPathFreeOrTimeOutRoutine(plannedPath);

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
		_pathFinder.Path = null;
	}

	public float CalculateMaxJumpDistance()
	{
		var jumpDirection = Quaternion.AngleAxis(-_jumpAngle, Vector3.right) * Vector3.forward;
		var initialVelocity = jumpDirection * _jumpForce;
		var maxJumpDistance = -2f * initialVelocity.y * initialVelocity.z / Physics.gravity.y;

		return maxJumpDistance;
	}

	private void CalculateTrajectory(float jForce, float jumpAngle)
	{
		var g = Physics.gravity.y;

		jumpAngle = _jumpForward ? -jumpAngle : 180f + jumpAngle;

		var jumpDirection = Quaternion.AngleAxis(jumpAngle, _cachedTransform.right) * _cachedTransform.forward;
		_initialVelocity = jumpDirection * jForce;
		_jumpDuration = -2f * _initialVelocity.y / g;

		_trajectory.Clear();

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
			_pathFinder.Path = _trajectory.GetRange(elapsedTrajectorySections, remainingSectionCount);
		}
	}
}

