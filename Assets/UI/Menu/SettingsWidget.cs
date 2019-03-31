using UnityEngine;
using UnityEngine.UI;

public class SettingsWidget : MonoBehaviour
{
	public Toggle invertMouseToggle;
	public Slider sensitivitySlider;

	private bool _isDirty;

	private void OnEnable()
	{
		Refresh();
		_isDirty = false;
	}

	private void OnDisable()
	{
		if (_isDirty)
		{
			ManagerLocator.TryGet<SettingsManager>().SetSettings(invertMouseToggle.isOn, sensitivitySlider.value);
			_isDirty = false;
		}
	}

	public void HandleValuesChanged()
	{
		_isDirty = true;
	}

	private void Refresh()
	{
		invertMouseToggle.SetIsOnWithoutNotify(SettingsManager.IsMouseInverted());
		sensitivitySlider.SetValueWithoutNotify(SettingsManager.MouseSensitivity());
	}
}
