using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMover : MonoWithCachedTransform
{
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

	private void Start()
	{
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
		while (true)
		{
			var jAngle = _jumpForward ? -jumpAngle : 180f + jumpAngle;
			var forward = CachedTransform.forward;

			var rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(jAngle, CachedTransform.right));
			var initialVelocity = rotation.MultiplyVector(forward).normalized * jumpForce;
			var totalTime = -2f * initialVelocity.y / Physics.gravity.y;
			var endPoint = CachedTransform.position + new Vector3(initialVelocity.x * totalTime, 0f, initialVelocity.z * totalTime);
			var elapsed = 0f;
			var startingPoint = CachedTransform.position;

			while (elapsed < totalTime)
			{
				elapsed += Time.fixedDeltaTime;

				var dx = initialVelocity.x * elapsed;
				var dy = initialVelocity.y * elapsed + Physics.gravity.y * elapsed * elapsed / 2f;
				var dz = initialVelocity.z * elapsed;

				CachedTransform.position = startingPoint + new Vector3(dx, dy, dz);
				meshToRotate.localRotation = Quaternion.Euler(90f * (_jumpForward ? 1f: -1f) * (elapsed / totalTime), 0f, 0f);
				yield return null;
			}

			CachedTransform.position = endPoint;
			_jumpForward = !_jumpForward;
			_shouldNotRedrawTrajectory = false;
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

		for (int i = 0; i <= trajectoryMarkerCount; ++i)
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

	private IEnumerator RollForwardRoutine()
	{
		while (true)
		{
			var fromEdgeToCentre = new Vector3(0.0f, 0.5f, 0.0f) - CachedTransform.forward.normalized * 0.5f;
			
			var axisToRotateAround = CachedTransform.right;
			var matrix = MatrixToRotateAboutAxisByAngles(axisToRotateAround.normalized, 0.5f);
			var anglesRotated = 0f;
			
			while (anglesRotated < 90f)
			{
				var delta = CachedTransform.position - fromEdgeToCentre;
				fromEdgeToCentre = matrix.MultiplyPoint3x4(fromEdgeToCentre);
				CachedTransform.position = fromEdgeToCentre + delta;
				anglesRotated += 0.5f;

				meshToRotate.Rotate(new Vector3(0.5f, 0f, 0f), Space.Self);

				yield return null;
			}

			CachedTransform.position = new Vector3(CachedTransform.position.x, 0.5f, CachedTransform.position.z);
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
