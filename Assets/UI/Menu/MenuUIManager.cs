using System.Collections;
using UnityEngine;

public class MenuUIManager : MonoBehaviour, IManager
{
	public GameObject mainMenu;
	public GameObject howToPlay;
	public ConfirmationPopup confirmPopup;
	public GameOverPopup gameOverPopup;

	public GameObject[] menuItems;

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

	private void HandleGameOver(bool playerHasWon)
	{
		StartCoroutine(GameOverRoutine(playerHasWon));
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

	private void HideAllMenuItems()
	{
		foreach (var item in menuItems)
		{
			item.gameObject.SetActive(false);
		}
	}

	private IEnumerator GameOverRoutine(bool playerHasWon)
	{
		var gc = ManagerLocator.TryGet<GameController>();

		gc.PauseGame(true);

		var context = new ConfirmationContext();
		gameOverPopup.Show(playerHasWon, context);

		while (!context.IsFinished)
		{
			yield return null;
		}

		gc.AbortGame(playerHasWon);

		ShowMainMenu();
	}

	private IEnumerator PauseGameRoutine(GameController gc)
	{
		gc.PauseGame(true);

		var context = new ConfirmationContext();
		confirmPopup.Setup("Back to the main menu?", context);

		while (!context.IsFinished)
		{
			yield return null;
		}

		if (context.IsConfirmed)
		{
			gc.AbortGame(hasPlayerWon: false);
			ShowMainMenu();
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
		ManagerLocator.TryGet<GameController>().Teardown();

		if (Application.isEditor)
		{
			UnityEditor.EditorApplication.isPlaying = false;
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
