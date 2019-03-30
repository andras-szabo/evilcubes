using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(DealDamageOnImpact))]
public class EnemyBody : MonoWithCachedTransform
{
	public event Action<HPInfo> OnHitPointsChanged;

	public EnemyComposingPart bodyPartPrototype;

	public List<EnemyComposingPart> composingParts = new List<EnemyComposingPart>();
	public DealDamageOnImpact dealDamageOnImpact;

	private int _maxHP;
	private int _hp;

	public void SetVisible(bool state)
	{
		foreach (var part in composingParts)
		{
			part.materialFader.MeshRenderer.enabled = state;
		}
	}

	public void Setup(EnemyConfig config)
	{
		SpawnAndSetupComposingCubes(config);
		dealDamageOnImpact.BoxCollider.size = new Vector3(config.edgeSize, config.edgeSize, config.edgeSize);
		_maxHP = _hp;
	}

	private void SpawnAndSetupComposingCubes(EnemyConfig config)
	{
		var cubesToSpawn = config.sectionCount * config.sectionCount * config.sectionCount;
		var unitCubeLength = config.edgeSize / config.sectionCount;
		var halfUnitCubeLength = unitCubeLength / 2f;
		var centre = CachedTransform.position;

		var startCoord = (config.sectionCount - 1) * halfUnitCubeLength;
		var nextPartSpawnPosition = new Vector3(-startCoord, -startCoord, -startCoord);
		var partScale = new Vector3(unitCubeLength, unitCubeLength, unitCubeLength);

		for (int i = 0; i < cubesToSpawn; ++i)
		{
			for (int j = 0; j < cubesToSpawn; ++j)
			{
				for (int k = 0; k < cubesToSpawn; ++k)
				{
					var cube = Instantiate<EnemyComposingPart>(bodyPartPrototype, centre + nextPartSpawnPosition, 
															   CachedTransform.rotation, CachedTransform);

					cube.CachedTransform.localScale = partScale;
					SetupComposingPart(cube, config);

					nextPartSpawnPosition += new Vector3(0f, 0f, halfUnitCubeLength);
				}

				nextPartSpawnPosition = new Vector3(nextPartSpawnPosition.x, 
													nextPartSpawnPosition.y + halfUnitCubeLength, 
													-startCoord);
			}

			nextPartSpawnPosition = new Vector3(nextPartSpawnPosition.x + halfUnitCubeLength,
												-startCoord, -startCoord);
		}
	}

	private void HandlePartsHPChanged(HPInfo info)
	{
		_hp -= (info.max - info.current);
		OnHitPointsChanged?.Invoke(new HPInfo { max = _maxHP, current = _hp });
	}

	private void SetupComposingPart(EnemyComposingPart part, EnemyConfig config)
	{
		composingParts.Add(part);

		var hp = part.hp;

		hp.SetStartingHP(config.CalculateHPPerComposingPart());
		hp.destroyWhenHPzero = true;
		hp.OnHitPointsChanged += HandlePartsHPChanged;

		_hp += hp.hitPoints;

		var fader = part.materialFader;
		fader.SetupRendererAndStartColor(config.color);
		fader.MeshRenderer.enabled = false;

		fader.SetupHPToObserve(hp);
	}

	private void SetupDamageOnImpact(EnemyConfig config)
	{
		if (dealDamageOnImpact != null)
		{
			dealDamageOnImpact.damage = config.damageOnImpact;
		}
	}
}
