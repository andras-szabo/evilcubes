using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public Direction RollDirection { get; set; }

	public RollMove(Transform cachedTransform, Transform mesh, PathFinder pathFinder, float edgeSize, float rollAnglePerUpdate)
	{
		_cachedTransform = cachedTransform;
		_meshToRotate = mesh;
		_edgeSize = edgeSize;
		_rollAnglePerUpdate = rollAnglePerUpdate;
		_pathFinder = pathFinder;
		_plannedPath = new List<Vector3> { Vector3.zero };

		RollDirection = Direction.Forward;
	}

	public IEnumerator Execute()
	{
		CalculatePath(RollDirection, _edgeSize);
		yield return WaitUntilPathFreeOrTimeOutRoutine(_plannedPath, 2f);

		if (!_lastPathFindWasSuccessful)
		{
			yield break;
		}

		var halfSize = _edgeSize / 2f;
		Vector3 axisToRotateAround;
		var fromEdgeToCentre = CalculateVectorFromEdgeToCentre(RollDirection, halfSize, out axisToRotateAround);

		var rollAngle = _rollAnglePerUpdate;
		if (RollDirection == Direction.Back || RollDirection == Direction.Right)
		{
			rollAngle *= -1f;
		}

		var matrix = MatrixToRotateAboutAxisByAngles(axisToRotateAround, rollAngle);
		var anglesRotated = 0f;
		var elapsed = 0f;
		var timeStep = Time.fixedDeltaTime;

		Vector3 meshRotationAxis = _cachedTransform.right;
		if (RollDirection == Direction.Right || RollDirection == Direction.Left)
		{
			meshRotationAxis = _cachedTransform.forward;
		}

		while (_rollAnglePerUpdate > 0f && Mathf.Abs(anglesRotated) < 90f)
		{
			elapsed += Time.deltaTime;

			while (elapsed > timeStep && Mathf.Abs(anglesRotated) < 90f)
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

				_meshToRotate.Rotate(meshRotationAxis, rollAngle, Space.World);
			}

			if (RollDirection == Direction.Left || RollDirection == Direction.Right)
			{
				YawToTarget(halfSize);
				meshRotationAxis = _cachedTransform.forward;
			}

			yield return null;
		}

		_cachedTransform.position = new Vector3(_cachedTransform.position.x, halfSize, _cachedTransform.position.z);

		if (RollDirection == Direction.Left || RollDirection == Direction.Right)
		{
			YawToTarget(halfSize);
		}

		_pathFinder.Path = null;
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
			case Direction.Left: axisToRotateAround = _cachedTransform.forward; return centre + _cachedTransform.right * halfSize;
			case Direction.Right: axisToRotateAround = _cachedTransform.forward; return centre -_cachedTransform.right * halfSize;
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

		_plannedPath[0] = target;
		return _plannedPath;
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
