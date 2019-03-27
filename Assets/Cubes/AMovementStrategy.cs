using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AMovementStrategy
{
	protected PathFinder _pathFinder;
	protected Transform _cachedTransform;
	protected Transform _meshToRotate;
	protected float _edgeSize;
	protected float _rollAnglePerUpdate;

	public AMovementStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, float edgeSize, float cubeSpeedUnitsPerSecond)
	{
		_cachedTransform = cachedTransform;
		_meshToRotate = meshToRotate;
		_pathFinder = pathFinder;
		_edgeSize = edgeSize;

		_rollAnglePerUpdate = CalculateRollAnglePerUpdate(edgeSize, cubeSpeedUnitsPerSecond);
	}

	public abstract IEnumerator RunRoutine();
	public abstract float GetMaxStepDistance();

	private float CalculateRollAnglePerUpdate(float edgeSize, float cubeSpeedUnitsPerSecond)
	{
		return cubeSpeedUnitsPerSecond / edgeSize * 90f * Time.fixedDeltaTime;
	}
}

public class JumpStrategy : AMovementStrategy
{
	private JumpMove _jump;
	private RollMove _roll;

	private float _jumpDistance;
	private float _jumpChance;

	public JumpStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, float edgeSize, float cubeSpeedUnitsPerSecond) :
		base(cachedTransform, meshToRotate, pathFinder, edgeSize, cubeSpeedUnitsPerSecond)
	{

		_jump = new JumpMove();
		//_roll = new RollMove(_cachedTransform, _meshToRotate, _edgeSize, _rollAnglePerUpdate);
	}

	public override float GetMaxStepDistance()
	{
		//TODO FIX
		return _edgeSize;
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
	private RollMove _roll;

	public RollStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, float edgeSize, float cubeSpeedUnitsPerSecond) :
				base(cachedTransform, meshToRotate, pathFinder, edgeSize, cubeSpeedUnitsPerSecond)
	{
		_roll = new RollMove(_cachedTransform, _meshToRotate, _pathFinder, _edgeSize, _rollAnglePerUpdate);
	}

	public override float GetMaxStepDistance()
	{
		return _edgeSize;
	}

	public override IEnumerator RunRoutine()
	{
		while (true)
		{
			//TODO
			yield return _roll.Execute();
		}
	}
}

public abstract class AMove
{
	protected PathFinder _pathFinder;

	private WaitForSeconds _pathCheckInterval = new WaitForSeconds(0.1f);

	protected int frameCounter;

	protected IEnumerator WaitUntilPathFreeRoutine(List<Vector3> path)
	{
		while (!_pathFinder.IsPathFree(path))
		{
			yield return _pathCheckInterval;
		}

		_pathFinder.Path = path;
	}
}

public class RollMove : AMove
{
	public enum Direction
	{
		None,

		Forward,
		Back,
		Left,
		Right
	}

	private readonly Transform _cachedTransform;
	private readonly Transform _meshToRotate;
	private readonly float _edgeSize;
	private readonly float _rollAnglePerUpdate;

	private Direction _direction = Direction.Forward;

	public RollMove(Transform cachedTransform, Transform mesh, PathFinder pathFinder, float edgeSize, float rollAnglePerUpdate)
	{
		_cachedTransform = cachedTransform;
		_meshToRotate = mesh;
		_edgeSize = edgeSize;
		_rollAnglePerUpdate = rollAnglePerUpdate;
		_pathFinder = pathFinder;
	}

	public IEnumerator Execute()
	{
		var plannedPath = CalculatePath(_direction, _edgeSize);
		yield return WaitUntilPathFreeRoutine(plannedPath);
		//Debug.LogFormat("{0} executing at: {1}", _cachedTransform.gameObject.name, Time.frameCount);

		var halfSize = _edgeSize / 2f;
		Vector3 axisToRotateAround;
		var fromEdgeToCentre = CalculateVectorFromEdgeToCentre(_direction, halfSize, out axisToRotateAround);

		var rollAngle = _rollAnglePerUpdate;
		if (_direction == Direction.Back || _direction == Direction.Right)
		{
			rollAngle *= -1f;
		}

		var matrix = MatrixToRotateAboutAxisByAngles(axisToRotateAround, rollAngle);
		var anglesRotated = 0f;
		var elapsed = 0f;
		var timeStep = Time.fixedDeltaTime;

		while (_rollAnglePerUpdate > 0f && anglesRotated < 90f)
		{
			elapsed += Time.deltaTime;

			while (elapsed > timeStep && anglesRotated < 90f)
			{
				elapsed -= timeStep;

				var delta = _cachedTransform.position - fromEdgeToCentre;
				fromEdgeToCentre = matrix.MultiplyPoint3x4(fromEdgeToCentre);
				_cachedTransform.position = fromEdgeToCentre + delta;
				anglesRotated += rollAngle;

				if (Mathf.Abs(anglesRotated) > 90f)
				{
					var overRotation = Mathf.Abs(anglesRotated) - 90f;
					if (rollAngle < 0f) { rollAngle += overRotation; }
					else { rollAngle -= overRotation; }
				}

				if (_direction == Direction.Forward || _direction == Direction.Back)
				{
					_meshToRotate.Rotate(new Vector3(rollAngle, 0f, 0f), Space.Self);
				}
				else
				{
					_meshToRotate.Rotate(new Vector3(0f, 0f, rollAngle), Space.Self);
				}
			}

			if (_direction == Direction.Left || _direction == Direction.Right)
			{
				YawToTarget(halfSize);
			}

			yield return null;
		}

		_cachedTransform.position = new Vector3(_cachedTransform.position.x, halfSize, _cachedTransform.position.z);

		if (_direction == Direction.Left || _direction == Direction.Right)
		{
			YawToTarget(halfSize);
		}

		_pathFinder.Path.Clear();
		//Debug.LogWarningFormat("{0} clears at {1}", _cachedTransform.gameObject.name, Time.frameCount);
	}

	private void YawToTarget(float cubeCentreHeight)
	{
		_cachedTransform.LookAt(new Vector3(0f, cubeCentreHeight, 0f), Vector3.up);
	}

	private Vector3 CalculateVectorFromEdgeToCentre(Direction direction, float halfSize,
						out Vector3 axisToRotateAround)
	{
		var centre = new Vector3(0f, halfSize, 0f);

		switch (direction)
		{
			case Direction.Forward: axisToRotateAround = _cachedTransform.right; return centre - _cachedTransform.forward * halfSize;
			case Direction.Back: axisToRotateAround = _cachedTransform.right; return centre + _cachedTransform.forward * halfSize;
			case Direction.Left: axisToRotateAround = _cachedTransform.forward; return _cachedTransform.right * halfSize;
			case Direction.Right: axisToRotateAround = _cachedTransform.forward; return -_cachedTransform.right * halfSize;
		}

		throw new System.NotImplementedException("Unknown rotation direction");
	}

	private List<Vector3> CalculatePath(Direction direction, float edgeSize)
	{
		Vector3 target = Vector3.zero;

		switch (direction)
		{
			case Direction.Forward: target = _cachedTransform.position + _cachedTransform.forward * edgeSize; break;
			case Direction.Back: target = _cachedTransform.position - _cachedTransform.forward * edgeSize; break;
			case Direction.Left: target = _cachedTransform.position - _cachedTransform.right * edgeSize; break;
			case Direction.Right: target = _cachedTransform.position + _cachedTransform.right * edgeSize; break;
		}

		return new List<Vector3> { target };
	}

	private Matrix4x4 MatrixToRotateAboutAxisByAngles(Vector3 n, float angle)
	{
		var angleInRadians = angle * Mathf.PI / 180f;
		var cosa = Mathf.Cos(angleInRadians);
		var sina = Mathf.Sin(angleInRadians);

		var col1 = new Vector4(n.x * n.x * (1f - cosa) + cosa, n.x * n.y * (1f - cosa) + n.z * sina, n.x * n.z * (1f - cosa) - n.y * sina, 0f);
		var col2 = new Vector4(n.x * n.y * (1f - cosa) - n.z * sina, n.y * n.y * (1f - cosa) + cosa, n.y * n.z * (1f - cosa) + n.x * sina, 0f);
		var col3 = new Vector4(n.x * n.z * (1f - cosa) + n.y * sina, n.y * n.z * (1f - cosa) - n.x * sina, n.z * n.z * (1f - cosa) + cosa, 0f);
		var col4 = new Vector4(0f, 0f, 0f, 1f);

		return new Matrix4x4(col1, col2, col3, col4);
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
		_pathFinder.Path.Clear();
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
			_pathFinder.Path = _trajectory.GetRange(elapsedTrajectorySections, remainingSectionCount);
		}
	}
}
