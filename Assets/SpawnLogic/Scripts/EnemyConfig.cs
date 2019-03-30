using UnityEngine;

[CreateAssetMenu()]
public class EnemyConfig : ScriptableObject
{
	public EnemyType type;
	[Range(1, 1000)] public int hitPoints;

	[Header("Rolls")]
	[Range(0.2f, 10f)] public float edgeSize;
	[Range(0.2f, 5f)] public float speedUnitsPerSecond;
	[Range(0f, 1f)] public float sideRollChance;

	[Header("Jumps")]
	[Range(10f, 100f)] public float jumpForce;
	[Range(20f, 85f)] public float jumpAngle;
	
	[Tooltip("A chance of 1 means a jumper will jump every move, unless the player is closer than a jump distance, at which point it will roll.")]
	[Range(0f, 1f)] public float jumpChance;

	[Header("Dmg")]
	[Range(0, 100)] public int damageOnImpact;

	[Header("Looks")]
	public Color color;
	[Tooltip("In case you'd like the enemy to be composed of smaller cubes")]
	[Range(1, 5)] public int sectionCount;

	[HideInInspector]
	public float halfBodyDiagonal;

	[HideInInspector]
	public int composingPartCount;

	public int CalculateHPPerComposingPart()
	{
		return (int)Mathf.Max(1, hitPoints / composingPartCount);
	}

	private void OnValidate()
	{
		halfBodyDiagonal = Mathf.Sqrt(3f) * edgeSize / 2f;
		edgeSize = Mathf.Max(0.2f, edgeSize);
		hitPoints = Mathf.Max(1, hitPoints);
		sectionCount = Mathf.Max(1, sectionCount);
		composingPartCount = (int) Mathf.Pow(sectionCount, 3);
		speedUnitsPerSecond = Mathf.Max(0.2f, speedUnitsPerSecond);
	}
}