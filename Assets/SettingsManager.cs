using System;
using UnityEngine;

public class SettingsManager : IManager
{
	public struct SettingsInfo
	{
		public bool isMouseInverted;
		public float mouseSensitivity;
	}

	public const string PPREF_KEY_INVERT_MOUSE = "invertMouse";
	public const string PPREF_KEY_MOUSE_SENSITIVITY = "mouseSensitivity";

	public static bool IsMouseInverted()
	{
		return PlayerPrefs.GetInt(PPREF_KEY_INVERT_MOUSE, 1) == 1;
	}

	public static float MouseSensitivity()
	{
		return PlayerPrefs.GetFloat(PPREF_KEY_MOUSE_SENSITIVITY, 200f);
	}

	public event Action<SettingsInfo> OnSettingsChanged;

	public void SetSettings(bool isInverted, float sensitivity)
	{
		PlayerPrefs.SetInt(PPREF_KEY_INVERT_MOUSE, isInverted ? 1 : 0);
		PlayerPrefs.SetFloat(PPREF_KEY_MOUSE_SENSITIVITY, sensitivity);

		OnSettingsChanged?.Invoke(new SettingsInfo { isMouseInverted = isInverted, mouseSensitivity = sensitivity });
	}
}
