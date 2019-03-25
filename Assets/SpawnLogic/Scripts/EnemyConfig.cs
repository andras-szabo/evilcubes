using UnityEngine;

[CreateAssetMenu()]
public class EnemyConfig : ScriptableObject
{
	public EnemyType type;
	[Range(0.2f, 10f)] public float edgeSize;

	[HideInInspector]
	public float halfBodyDiagonal;

	private void OnValidate()
	{
		halfBodyDiagonal = Mathf.Sqrt(3f) * edgeSize / 2f;
	}
}