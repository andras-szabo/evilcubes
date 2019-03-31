using System.Collections;
using UnityEngine;

public class BulletTrail : MonoWithCachedTransform
{
	public TrailRenderer trailRenderer;

	private void OnEnable()
	{
		trailRenderer?.Clear();
	}

	public void Setup(Vector3 targetPosition)
	{
		StartCoroutine(FlyToTargetPositionRoutine(targetPosition));
	}

	private IEnumerator FlyToTargetPositionRoutine(Vector3 target)
	{
		yield return null;
		CachedTransform.position = target;
	}
}
