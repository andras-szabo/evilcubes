using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CubeAwareness))]
public class TestMover : MonoWithCachedTransform
{
	public static float rollAngleSpeedPerFrame = 1f;
	public Transform meshToRotate;

	[Range(0f, 100f)]
	public float jumpForce = 10f;

	[Range(10f, 90f)]
	public float jumpAngle = 45f;

	public bool isJumper = false;

	private List<Vector3> trajectory = new List<Vector3>();
	private float _previousJumpForce;
	private float _previousJumpAngle;
	private bool _jumpForward = true;

	private bool _shouldNotRedrawTrajectory;

	private CubeAwareness _cubeAwareness;
	private CubeAwareness CubeAwareness
	{
		get
		{
			return _cubeAwareness ?? (_cubeAwareness = GetComponent<CubeAwareness>());
		}
	}

	private void Start()
	{
		CubeAwareness.UpdatePath(new List<Vector3> { CachedTransform.position });

		if (isJumper)
		{
			CalculateTrajectory(jumpForce, jumpAngle);
			_shouldNotRedrawTrajectory = true;

			StartCoroutine(BunnyHopRoutine());
		}
		else
		{
			StartCoroutine(RollForwardRoutine());
		}
	}

	private void Update()
	{
		if (CachedTransform.hasChanged || !Mathf.Approximately(_previousJumpForce, jumpForce) || !Mathf.Approximately(_previousJumpAngle, jumpAngle))
		{
			if (!_shouldNotRedrawTrajectory && isJumper)
			{
				_previousJumpForce = jumpForce;
				_previousJumpAngle = jumpAngle;

				CalculateTrajectory(jumpForce, jumpAngle);
				_shouldNotRedrawTrajectory = true;
			}
		}
	}

	private IEnumerator BunnyHopRoutine()
	{
		yield return new WaitForSeconds(0.5f);

		while (true)
		{
			var path = trajectory.GetRange(1, trajectory.Count - 1);
			yield return WaitUntilPathFreeRoutine(path, false);
			CubeAwareness.UpdatePath(path);

			var jAngle = _jumpForward ? -jumpAngle : 180f + jumpAngle;
			var forward = CachedTransform.forward;

			var rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(jAngle, CachedTransform.right));
			var initialVelocity = rotation.MultiplyVector(forward).normalized * jumpForce;
			var totalTime = -2f * initialVelocity.y / Physics.gravity.y;
			var endPoint = CachedTransform.position + new Vector3(initialVelocity.x * totalTime, 0f, initialVelocity.z * totalTime);
			var elapsed = 0f;
			var startingPoint = CachedTransform.position;

			var trajectorySectionCount = 16;
			var timeForOneSection = totalTime / trajectorySectionCount;
					   
			while (elapsed < totalTime)
			{
				elapsed += Time.fixedDeltaTime;

				var dx = initialVelocity.x * elapsed;
				var dy = initialVelocity.y * elapsed + Physics.gravity.y * elapsed * elapsed / 2f;
				var dz = initialVelocity.z * elapsed;

				CachedTransform.position = startingPoint + new Vector3(dx, dy, dz);
				meshToRotate.localRotation = Quaternion.Euler(90f * (_jumpForward ? 1f: -1f) * (elapsed / totalTime), 0f, 0f);

				var elapsedTrajectorySections = (int) (elapsed / timeForOneSection);
				if (elapsedTrajectorySections < 1)
				{
					elapsedTrajectorySections = 1;
				}
				var remaining = trajectorySectionCount - elapsedTrajectorySections;

				CubeAwareness.UpdatePath(trajectory.GetRange(elapsedTrajectorySections, remaining));

				yield return null;
			}

			CachedTransform.position = endPoint;
			_jumpForward = !_jumpForward;
			_shouldNotRedrawTrajectory = false;

			//TODO
			CubeAwareness.UpdatePath(new List<Vector3>());

			yield return null;
		}
	}

	private void CalculateTrajectory(float jForce, float jAngle)
	{
		var g = Physics.gravity.y;

		jAngle = _jumpForward ? -jumpAngle : 180f + jumpAngle;

		var rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(jAngle, CachedTransform.right));
		var initialVelocity = (rotation.MultiplyVector(CachedTransform.forward)).normalized * jForce;

		var totalTime = -2f * initialVelocity.y / g;

		trajectory = new List<Vector3>();
		var trajectoryMarkerCount = 16;

		for (int i = 1; i <= trajectoryMarkerCount; ++i)
		{
			var t = (totalTime / trajectoryMarkerCount) * i;

			var dx = initialVelocity.x * t;
			var dy = initialVelocity.y * t + g * t * t / 2f;
			var dz = initialVelocity.z * t;

			trajectory.Add(CachedTransform.position + new Vector3(dx, dy, dz));
		}
	}

	private IEnumerator RollSidewaysRoutine()
	{
		while (true)
		{
			var left = false;
			var halfSize = 0.5f;
			var fromEdgeToCentre = left ? CachedTransform.right * halfSize : -CachedTransform.right * halfSize;
			fromEdgeToCentre += new Vector3(0f, halfSize, 0f);

			var axisToRotateAround = CachedTransform.forward;
			var matrix = MatrixToRotateAboutAxisByAngles(axisToRotateAround.normalized, left ? 0.5f : -0.5f);
			var anglesRotated = 0f;

			while (anglesRotated < 90f)
			{
				var delta = CachedTransform.position - fromEdgeToCentre;
				fromEdgeToCentre = matrix.MultiplyPoint3x4(fromEdgeToCentre);
				CachedTransform.position = fromEdgeToCentre + delta;
				anglesRotated += 0.5f;
				meshToRotate.Rotate(new Vector3(0f, 0f, left ? 0.5f : -0.5f), Space.Self);
				yield return null;
			}

			CachedTransform.position = new Vector3(CachedTransform.position.x, 0.5f, CachedTransform.position.z);
		}
	}

	private IEnumerator WaitUntilPathFreeRoutine(IEnumerable<Vector3> path, bool log = false)
	{
		var checkInterval = new WaitForSeconds(0.1f);
		while (!CubeAwareness.IsPathFree(path, 0.5f, log))
		{
			yield return checkInterval;
		}
	}

	private IEnumerator RollForwardRoutine()
	{
		yield return new WaitForSeconds(0.2f);

		while (true)
		{
			var path = new List<Vector3>
			{
				CachedTransform.position + CachedTransform.forward * 1f
			};

			yield return WaitUntilPathFreeRoutine(path);

			CubeAwareness.UpdatePath(new List<Vector3> { path[0] });

			var fromEdgeToCentre = new Vector3(0.0f, 0.5f, 0.0f) - CachedTransform.forward.normalized * 0.5f;
			
			var axisToRotateAround = CachedTransform.right;
			var matrix = MatrixToRotateAboutAxisByAngles(axisToRotateAround.normalized, rollAngleSpeedPerFrame);
			var anglesRotated = 0f;
			
			while (anglesRotated < 90f)
			{
				var delta = CachedTransform.position - fromEdgeToCentre;
				fromEdgeToCentre = matrix.MultiplyPoint3x4(fromEdgeToCentre);
				CachedTransform.position = fromEdgeToCentre + delta;
				anglesRotated += rollAngleSpeedPerFrame;

				meshToRotate.Rotate(new Vector3(rollAngleSpeedPerFrame, 0f, 0f), Space.Self);

				yield return null;
			}

			CachedTransform.position = new Vector3(CachedTransform.position.x, 0.5f, CachedTransform.position.z);

			//TODO ---- formalized, nicely
			CubeAwareness.UpdatePath(new List<Vector3>());
		}
	}

	private void OnDrawGizmos()
	{
		if (Application.isPlaying)
		{
			DrawTrajectory();
		}
	}

	private void DrawTrajectory()
	{
		Gizmos.color = Color.blue;
		foreach (var pos in trajectory)
		{
			Gizmos.DrawSphere(pos, 0.2f);
		}
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
