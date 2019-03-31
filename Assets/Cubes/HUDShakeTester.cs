using UnityEngine;

public class HUDShakeTester : MonoBehaviour
{
	[Range(0.1f, 10f)] public float intensity;

	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			foreach (var shakeable in ManagerLocator.TryGetAll<IShakeable>())
			{
				shakeable.Shake(intensity);
			}

			Invoke("MakePlayerDie", 0.1f);
		}
	}

	private void MakePlayerDie()
	{
		ManagerLocator.TryGet<PlayerController>().HP.Hit(Vector3.zero, 200);
	}
}
