using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialFader : MonoBehaviour
{
	public HP hpToObserve;

	public Color color;

	private MaterialPropertyBlock _materialPropertyBlock;

	private Renderer _renderer;
	public Renderer MeshRenderer
	{
		get
		{
			return _renderer ?? (_renderer = GetComponent<Renderer>());
		}
	}

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
		_materialPropertyBlock = new MaterialPropertyBlock();

		MeshRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", color);
		MeshRenderer.SetPropertyBlock(_materialPropertyBlock);
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
