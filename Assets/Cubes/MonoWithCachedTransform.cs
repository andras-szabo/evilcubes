using UnityEngine;

public class MonoWithCachedTransform : MonoBehaviour 
{
	protected Transform _cachedTransform;
	public Transform CachedTransform
	{
		get
		{
			return _cachedTransform ?? (_cachedTransform = this.transform);
		}
	}
}
