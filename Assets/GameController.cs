using UnityEngine;

public class GameController : MonoBehaviour 
{
	//TODO
	private void Awake()
	{
		Cursor.lockState = CursorLockMode.Locked;
	}

	private void OnDestroy()
	{
		ManagerLocator.Cleanup();
	}
}
