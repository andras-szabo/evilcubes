using UnityEngine;
using UnityEngine.UI;

public class GameOverPopup : MonoBehaviour
{
	public Text label;
	private ConfirmationContext _context;

	public void Show(bool hasPlayerWon, ConfirmationContext context)
	{
		_context = context;
		var congratulations = hasPlayerWon ? "Yay, you've won!, so great!" :
											 "Yay, you've ALMOST won! Neat!";

		label.text = congratulations;
		gameObject.SetActive(true);
	}

	public void OnOKClicked()
	{
		_context.Confirm();
		gameObject.SetActive(false);
	}
}
