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

	private List<Vector3> trajectory = new List<Vector3>();
	private float _previousJumpForce;
	private float _previousJumpAngle;

	private void Start()
	{
		//StartCoroutine(RollToSideRoutine());
	}

	private void Update()
	{
		if (CachedTransform.hasChanged || !Mathf.Approximately(_previousJumpForce, jumpForce) || !Mathf.Approximately(_previousJumpAngle, jumpAngle))
		{
			_previousJumpForce = jumpForce;
			_previousJumpAngle = jumpAngle;

			CalculateTrajectory(jumpForce, jumpAngle);
		}
	}

	private void CalculateTrajectory(float jumpForce, float jumpAngle)
	{
		var g = Physics.gravity.y;

		var rotation = Matrix4x4.Rotate(Quaternion.AngleAxis(-jumpAngle, CachedTransform.right));
		var initialVelocity = (rotation.MultiplyVector(CachedTransform.forward)).normalized * jumpForce;

		var totalTime = -2f * initialVelocity.y / g;

		trajectory = new List<Vector3>();
		var trajectoryMarkerCount = 16;

		for (int i = 0; i <= trajectoryMarkerCount; ++i)
		{
			var t = totalTime / trajectoryMarkerCount * i;

			var dx = initialVelocity.x * t;
			var dy = initialVelocity.y * t + g * t * t / 2f;
			var dz = initialVelocity.z * t;

			trajectory.Add(CachedTransform.position + new Vector3(dx, dy, dz));
		}
	}

	private IEnumerator TestRoutine()
	{
		while (true)
		{
			var axisToRotateAround = CachedTransform.right;
			var pointOnAxis = CachedTransform.position + new Vector3(0f, -0.5f, 0.5f);
			var anglesRotated = 0.0f;
			while (anglesRotated < 90f)
			{
				CachedTransform.RotateAround(pointOnAxis, CachedTransform.right, 0.5f);
				anglesRotated += 0.5f;
				yield return null;
			}

			CachedTransform.position = new Vector3(CachedTransform.position.x, 0.5f, CachedTransform.position.z);
		}
	}

	private IEnumerator RollToSideRoutine()
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

	private IEnumerator TestRoutine2()
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
