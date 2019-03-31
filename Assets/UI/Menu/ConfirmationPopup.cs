using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopup : MonoBehaviour 
{
	public Text label;
	private ConfirmationContext _context;

	public void Setup(string thingToConfirm, ConfirmationContext context)
	{
		label.text = thingToConfirm;
		_context = context;
		gameObject.SetActive(true);
	}

	public void OnCancelClicked()
	{
		_context.Cancel();
		Teardown();
	}

	public void OnConfirmClicked()
	{
		_context.Confirm();
		Teardown();
	}

	private void Teardown()
	{
		_context = null;
		gameObject.SetActive(false);
	}
}
