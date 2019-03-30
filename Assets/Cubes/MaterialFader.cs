using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class MaterialFader : MonoBehaviour
{
	public HP hpToObserve;

	public bool selfSetup;
	public Color defaultColor;

	private MaterialPropertyBlock _materialPropertyBlock;
	private Color _startColor;

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

		if (selfSetup)
		{
			SetupRendererAndStartColor(defaultColor);
		}
	}

	public void SetupHPToObserve(HP hp)
	{
		hpToObserve = hp;
		ObserveHP();
	}

	private void ObserveHP()
	{
		if (hpToObserve != null)
		{
			hpToObserve.OnHitPointsChanged += HandleHitPointsChanged;
		}
	}

	public void SetupRendererAndStartColor(Color newColor)
	{
		if (_materialPropertyBlock == null)
		{
			_materialPropertyBlock = new MaterialPropertyBlock();
		}

		MeshRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", newColor);
		MeshRenderer.SetPropertyBlock(_materialPropertyBlock);

		_startColor = newColor;
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
		_materialPropertyBlock.SetColor("_Color", new Color(_startColor.r, _startColor.g, _startColor.b, currentOpacity));
		_renderer.SetPropertyBlock(_materialPropertyBlock);
	}
}
