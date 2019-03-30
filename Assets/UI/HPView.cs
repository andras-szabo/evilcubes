using UnityEngine;
using UnityEngine.UI;

public class HPView : MonoBehaviour
{
	[Tooltip("If not set, will track HP of the player.")]
	public HP hpToObserve;

	public Text label;
	public Image fillbar;

	public Color maxHealthColor;
	public Color minHealthColor;

	private void Awake()
	{
		label.text = string.Empty;
		fillbar.fillAmount = 0f;
	}

	private void Start()
	{
		if (hpToObserve == null)
		{
			TryFindPlayerHP();
		}

		if (hpToObserve != null)
		{
			hpToObserve.OnHitPointsChanged += HandleHPChanged;
			HandleHPChanged(hpToObserve.GetHPInfo());
		}
	}

	private void HandleHPChanged(HPInfo info)
	{
		label.text = string.Format("HP: {0} / {1}", info.current, info.max);
		fillbar.fillAmount = info.RateToFull;
		fillbar.color = Color.Lerp(minHealthColor, maxHealthColor, info.RateToFull);
	}

	private void OnDestroy()
	{
		if (hpToObserve != null)
		{
			hpToObserve.OnHitPointsChanged -= HandleHPChanged;
		}
	}

	private void TryFindPlayerHP()
	{
		var player = ManagerLocator.TryGet<PlayerController>();
		if (player != null)
		{
			hpToObserve = player.HP;
		}
	}
}
