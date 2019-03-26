using UnityEngine;

[CreateAssetMenu()]
public class EnemyConfig : ScriptableObject
{
	public EnemyType type;
	[Range(0.2f, 10f)] public float edgeSize;
	[Range(0.2f, 5f)] public float speedUnitsPerSecond;

	[HideInInspector]
	public float halfBodyDiagonal;

	private void OnValidate()
	{
		halfBodyDiagonal = Mathf.Sqrt(3f) * edgeSize / 2f;
		edgeSize = Mathf.Max(0.2f, edgeSize);
		speedUnitsPerSecond = Mathf.Max(0.2f, speedUnitsPerSecond);
	}
}