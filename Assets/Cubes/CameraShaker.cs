using UnityEngine;

public class CameraShaker : MonoWithCachedTransform
{
	public const float MAX_INTENSITY_FACTOR = 1f;
	public const float MIN_INTENSITY_FACTOR = 0.2f;

	public const float MAX_DISTANCE = 80f;

	public void ShakeIt(float shakeIntensity)
	{
		// Player is assumed to be at the origin
		var flatPosition = new Vector2(CachedTransform.position.x, CachedTransform.position.z);
		var distanceToPlayer = flatPosition.magnitude;

		var intensity = shakeIntensity * Mathf.Lerp(MAX_INTENSITY_FACTOR, MIN_INTENSITY_FACTOR, 
													distanceToPlayer / MAX_DISTANCE);

		var shakeables = ManagerLocator.TryGetAll<IShakeable>();

		if (shakeables != null && shakeables.Count > 0)
		{
			foreach (var shakeable in shakeables)
			{
				shakeable.Shake(intensity);
			}
		}
	}
}
