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
	private int _previousHP;

	public void SetVisible(bool state)
	{
		foreach (var part in composingParts)
		{
			part.hp.enabled = state;
			part.materialFader.MeshRenderer.enabled = state;
		}
	}

	public void Setup(EnemyConfig config)
	{
		SpawnAndSetupComposingCubes(config);

		dealDamageOnImpact.BoxCollider.size = new Vector3(config.edgeSize, config.edgeSize, config.edgeSize);
		dealDamageOnImpact.damage = config.damageOnImpact;

		_hp = config.hitPointsPerPart * config.composingPartCount;
		_maxHP = _hp;
	}

	private void SpawnAndSetupComposingCubes(EnemyConfig config)
	{
		var unitCubeLength = config.edgeSize / config.sectionCount;
		var halfUnitCubeLength = unitCubeLength / 2f;

		var delta = (config.sectionCount - 1) * halfUnitCubeLength;
		var startP = -CachedTransform.forward * delta - CachedTransform.right * delta - CachedTransform.up * delta;

		var right = CachedTransform.right * unitCubeLength;
		var back = CachedTransform.forward * unitCubeLength;
		var up = CachedTransform.up * unitCubeLength;

		var partScale = new Vector3(unitCubeLength, unitCubeLength, unitCubeLength);

		for (int i = 0; i < config.sectionCount; ++i)
		{
			for (int j = 0; j < config.sectionCount; ++j)
			{
				for (int k = 0; k < config.sectionCount; ++k)
				{
					var v = startP + right * k + up * j + back * i;
					var cube = Instantiate<EnemyComposingPart>(bodyPartPrototype, CachedTransform.position + v,
															   CachedTransform.rotation, CachedTransform);

					cube.CachedTransform.localScale = partScale;
					SetupComposingPart(cube, config);
				}
			}
		}
	}

	private void HandlePartsHPChanged(HPInfo info)
	{
		_previousHP = _hp;
		_hp -= (info.previous - info.current);
		OnHitPointsChanged?.Invoke(new HPInfo { max = _maxHP, current = _hp, previous = _previousHP });
	}

	private void SetupComposingPart(EnemyComposingPart part, EnemyConfig config)
	{
		composingParts.Add(part);

		var hp = part.hp;

		hp.SetStartingHP(config.hitPointsPerPart);
		hp.destroyWhenHPzero = true;
		hp.OnHitPointsChanged += HandlePartsHPChanged;

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
