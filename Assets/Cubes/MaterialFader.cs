using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialFader : MonoBehaviour 
{
	public HP hpToObserve;

	public Color color;
	private Renderer _renderer;
	private MaterialPropertyBlock _materialPropertyBlock;

	private void Start()
	{
		ObserveHP();
		SetupRendererAndStartColor();
	}

	private void ObserveHP()
	{
		if (hpToObserve != null)
		{
			hpToObserve.OnHitPointsChanged += HandleHitPointsChanged;
		}
	}

	private void SetupRendererAndStartColor()
	{
		_renderer = GetComponent<Renderer>();
		_materialPropertyBlock = new MaterialPropertyBlock();

		_renderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", color);
		_renderer.SetPropertyBlock(_materialPropertyBlock);
	}

	private void OnDestroy()
	{
		if (hpToObserve != null)
		{
			hpToObserve.OnHitPointsChanged -= HandleHitPointsChanged;
		}
	}

	private void HandleHitPointsChanged(HPInfo hpInfo)
	{
		var currentOpacity = hpInfo.RateToFull;

		_renderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", new Color(color.r, color.g, color.b, currentOpacity));
		_renderer.SetPropertyBlock(_materialPropertyBlock);
	}
}
