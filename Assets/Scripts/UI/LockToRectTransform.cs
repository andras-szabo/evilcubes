using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockToRectTransform : MonoBehaviour 
{
	public RectTransform rectTransformToFollow;

	private RectTransform _rt;
	private RectTransform CachedRectTransform
	{
		get
		{
			return _rt ?? (_rt = GetComponent<RectTransform>());
		}
	}

	private void LateUpdate ()
	{
		if (rectTransformToFollow.hasChanged)
		{
			CachedRectTransform.position = rectTransformToFollow.position;
		}
	}
}
