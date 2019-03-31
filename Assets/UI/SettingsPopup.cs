using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPopup : MonoBehaviour
{
	public Button leftButton;
	public Button rightButton;
	public Text leftButtonLabel;
	public Text rightButtonLabel;

	private Action _onLeftButtonClick;
	private Action _onRightButtonClick;

	public void ShowInGame(ConfirmationContext doesUserWantToQuit)
	{
		_onLeftButtonClick = () =>
		{
			gameObject.SetActive(false);
			doesUserWantToQuit.Cancel();
		};

		leftButtonLabel.text = "OK";
		leftButton.gameObject.SetActive(true);

		_onRightButtonClick = () => StartQuitConfirmFlow(doesUserWantToQuit);
		rightButtonLabel.text = "Quit";
		rightButton.gameObject.SetActive(true);

		gameObject.SetActive(true);
	}

	private void StartQuitConfirmFlow(ConfirmationContext doesUserWantToQuit)
	{
		StartCoroutine(QuitConfirmationFlowRoutine(doesUserWantToQuit));
	}

	private IEnumerator QuitConfirmationFlowRoutine(ConfirmationContext doesUserWantToQuit)
	{
		var menuManager = ManagerLocator.TryGet<MenuUIManager>();
		var quitConfirm = new ConfirmationContext();
		menuManager.ShowConfirmPopup("Back to main menu?", quitConfirm);

		while (!quitConfirm.IsFinished)
		{
			yield return null;
		}

		if (quitConfirm.IsConfirmed)
		{
			gameObject.SetActive(false);
			doesUserWantToQuit.Confirm();
		}
	}

	public void ShowWhileAtMenu()
	{
		_onLeftButtonClick = () => gameObject.SetActive(false);
		leftButtonLabel.text = "OK";
		leftButton.gameObject.SetActive(true);

		rightButton.gameObject.SetActive(false);

		gameObject.SetActive(true);
	}

	public void OnLeftButtonClick()
	{
		_onLeftButtonClick?.Invoke();
	}

	public void OnRightButtonClick()
	{
		_onRightButtonClick?.Invoke();
	}
}
