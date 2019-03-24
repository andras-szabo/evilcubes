using UnityEngine;

[CreateAssetMenu()]
public class WeaponConfig : ScriptableObject
{
	[Header("ID")]
	public string id;

	[Header("Firing rate and range")]
	public bool isAutomatic;
	[Range(0f, 5f)] public float coolDownSeconds;
	[Range(1f, 500f)] public float range;

	[Header("Bullet")]
	[Range(0.1f, 100f)] public float damagePerProjectile;
	[Tooltip("After each penetration, projectile damage is reduced by this factor. " +
			  "Set it to 1 to prevent penetration, and 0 to allow 'like hot knife through butter' behaviour.")]
	[Range(0f, 1f)] public float dmgReductionRate;

	[Header("Dispersion")]	
	[Range(1, 50)] public int projectileCountPerShot;
	[Range(0f, 20f)] public float dispersionDegrees;
	[Range(1f, 5f)] public float dispersionIncrementOverTime;
}
