using System.Collections;
using UnityEngine;

public class MenuUIManager : MonoBehaviour, IManager
{
	public GameObject mainMenu;
	public GameObject howToPlay;
	public SettingsPopup settingsPopup;
	public ConfirmationPopup confirmPopup;
	public GameOverPopup gameOverPopup;

	public GameObject[] menuItems;

	public void Cleanup()
	{
		StopAllCoroutines();
	}

	private void Awake()
	{
		ManagerLocator.TryRegister<MenuUIManager>(this);
	}

	private void Start()
	{
		ManagerLocator.TryGet<GameController>().OnGameOver += HandleGameOver;
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
		{
			var gc = ManagerLocator.TryGet<GameController>();
			if (gc != null && gc.IsPlaying)
			{
				StartCoroutine(PauseGameRoutine(gc));
			}
		}
	}

	private void OnDestroy()
	{
		var gc = ManagerLocator.TryGet<GameController>();
		if (gc != null)
		{
			gc.OnGameOver -= HandleGameOver;
		}
	}

	private void HandleGameOver(GameController.GameResult gameResult)
	{
		StartCoroutine(GameOverRoutine(gameResult));
	}

	public void OnSettingsButtonClicked()
	{
		settingsPopup.ShowWhileAtMenu();
	}

	public void OnHowToPlayButtonClicked()
	{
		howToPlay.SetActive(true);
	}

	public void OnQuitClicked()
	{
		StartCoroutine(QuitConfirmRoutine());
	}

	public void OnNewGameButtonClicked()
	{
		HideAllMenuItems();
		ManagerLocator.TryGet<GameController>().StartNewGame();
	}

	public void ShowConfirmPopup(string text, ConfirmationContext confirmContext)
	{
		confirmPopup.Setup(text, confirmContext);
	}

	private void HideAllMenuItems()
	{
		foreach (var item in menuItems)
		{
			item.gameObject.SetActive(false);
		}
	}

	private IEnumerator GameOverRoutine(GameController.GameResult playerHasWon)
	{
		var gc = ManagerLocator.TryGet<GameController>();

		gc.PauseGame(true);

		if (playerHasWon != GameController.GameResult.PlayerQuit)
		{
			var playerWon = playerHasWon == GameController.GameResult.PlayerWon;
			var context = new ConfirmationContext();
			gameOverPopup.Show(playerWon, context);

			while (!context.IsFinished)
			{
				yield return null;
			}
		}

		gc.AbortGameServices();

		ShowMainMenu();
	}

	private IEnumerator PauseGameRoutine(GameController gc)
	{
		gc.PauseGame(true);

		var doesUserWantToQuit = new ConfirmationContext();
		settingsPopup.ShowInGame(doesUserWantToQuit);

		while (!doesUserWantToQuit.IsFinished)
		{
			yield return null;
		}

		if (doesUserWantToQuit.IsConfirmed)
		{
			gc.HandlePlayerQuit();
		}
		else
		{
			gc.PauseGame(false);
		}
	}

	private IEnumerator QuitConfirmRoutine()
	{
		var context = new ConfirmationContext();
		confirmPopup.Setup("Are you sure you want to quit?", context);

		while (!context.IsFinished)
		{
			yield return null;
		}

		if (context.IsConfirmed)
		{
			DoQuit();
		}
	}

	private void ShowMainMenu()
	{
		mainMenu.gameObject.SetActive(true);
	}

	private void DoQuit()
	{
		if (Application.isEditor)
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
		else
		{
			Application.Quit();
		}
	}
}

public class ConfirmationContext
{
	public bool IsFinished { get; private set; }
	public bool IsConfirmed { get; private set; }

	public object parameters;

	public void Confirm()
	{
		IsConfirmed = true;
		IsFinished = true;
	}

	public void Cancel()
	{
		IsConfirmed = false;
		IsFinished = true;
	}

}
