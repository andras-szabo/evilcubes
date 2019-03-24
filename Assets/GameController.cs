using UnityEngine;

public class GameController : MonoBehaviour 
{
	//TODO
	private void OnDestroy()
	{
		ManagerLocator.Cleanup();
	}
}
