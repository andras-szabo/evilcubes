using System.Collections;
using UnityEngine;

public class TestMover : MonoWithCachedTransform
{
	public Transform meshToRotate;

	private void Start()
	{
		StartCoroutine(TestRoutine2());
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

	private IEnumerator TestRoutine2()
	{
		while (true)
		{
			var fromEdgeToCentre = new Vector3(0.0f, 0.5f, 0.0f) - CachedTransform.forward.normalized * 0.5f;
			
			var axisToRotateAround = CachedTransform.right;
			var pointOnAxis = (CachedTransform.position - fromEdgeToCentre + axisToRotateAround).normalized;

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
			Gizmos.color = Color.blue;
			Gizmos.DrawSphere(CachedTransform.position, 0.1f);
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
