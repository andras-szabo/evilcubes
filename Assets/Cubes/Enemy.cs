using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearbyCubeTracker))]
public class Enemy : MonoWithCachedTransform
{
	public event Action<Enemy> OnRemoved;

	public DealDamageOnImpact dealDamageOnImpact;

	//TODO better name
	public MaterialFader materialFader;
	public PathFinder PathFinder { get; private set; }
	public bool IsSpawning { get; private set; }
	public float HalfEdgeSize { get; private set; }

	private Transform _bodyTransform;
	private Transform BodyTransform
	{
		get
		{
			return _bodyTransform ?? (_bodyTransform = materialFader.transform);
		}
	}

	private NearbyCubeTracker _nearbyCubeTracker;
	private NearbyCubeTracker CubeTracker
	{
		get
		{
			return _nearbyCubeTracker ?? (_nearbyCubeTracker = GetComponent<NearbyCubeTracker>());
		}
	}

	private AMovementStrategy _movementStrategy;
	private bool _isSetup;

	private void Awake()
	{
		materialFader.MeshRenderer.enabled = false;
	}

	private void Start()
	{
		var hp = materialFader.hpToObserve;
		if (hp != null)
		{
			hp.destroyWhenHPzero = false;
			hp.OnHitPointsChanged += HandleHpChanged;
		}
	}
	
	//TODO
	private void LateUpdate()
	{
		if (_isSetup && !IsSpawning && PathFinder.AmIOverlappingAnotherCube(HalfEdgeSize))
		{
			Debug.LogError(gameObject.name);
			Debug.Break();
		}
	}

	private void OnDestroy()
	{
		OnRemoved?.Invoke(this);
	}

	public void Setup(EnemyConfig config, float speedMultiplier = 1f)
	{
		PathFinder = new PathFinder(CachedTransform, CubeTracker, config.halfBodyDiagonal);
		PickMovementStrategy(config, speedMultiplier);
		CubeTracker.UpdateTrackedAreaSize(_movementStrategy.GetMaxStepDistance());
		SetupDamageOnImpact(config);
		UpdateVisuals(config);

		HalfEdgeSize = config.edgeSize / 2f;
		_isSetup = true;

		IsSpawning = true;
		StartCoroutine(CheckPathAndStartMovingRoutine());	
	}

	private void SetupDamageOnImpact(EnemyConfig config)
	{
		if (dealDamageOnImpact != null)
		{
			dealDamageOnImpact.damage = config.damageOnImpact;
			dealDamageOnImpact.OnImpact += HandleImpact;
		}
	}

	private void HandleImpact()
	{
		Destroy(this.gameObject);
	}

	private void HandleHpChanged(HPInfo hp)
	{
		if (hp.current <= 0f)
		{
			Destroy(this.gameObject);
		}
	}

	private IEnumerator CheckPathAndStartMovingRoutine()
	{
		yield return new WaitForSeconds(0.5f);

		string otherName;
		if (PathFinder.IsMyPositionInAnotherPath(out otherName))
		{
			// Looks like we were spawned onto a position which is empty (or it was
			// in the last frame when I spawned), but it's in the path of someone
			// else, who now can't do anything to avoid collision. In this case,
			// let's pretend I wasn't even spawned.
			Destroy(this.gameObject);
		}
		else
		{
			materialFader.MeshRenderer.enabled = true;
			IsSpawning = false;
			yield return _movementStrategy.RunRoutine();
		}
	}

	private void UpdateVisuals(EnemyConfig config)
	{
		BodyTransform.localScale = new Vector3(config.edgeSize, config.edgeSize, config.edgeSize);
	}

	private void PickMovementStrategy(EnemyConfig config, float speedMultiplier = 1f)
	{
		//TODO
		switch (config.type)
		{
			case EnemyType.Simple: 
			case EnemyType.Zigzag:
			case EnemyType.Titan:
				_movementStrategy = new RollStrategy(CachedTransform, BodyTransform, PathFinder, config, speedMultiplier); 
				break;

			case EnemyType.Jumper:
				_movementStrategy = new JumpStrategy(CachedTransform, BodyTransform, PathFinder, config, speedMultiplier);
				break;
		}
	}

	public bool OverlapsAnyPositions(IEnumerable<Vector3> positions, float otherCubeSize)
	{
		return PathFinder.OverlapsAnyPositions(positions, otherCubeSize);
	}

	private void OnDrawGizmos()
	{
		PathFinder?.OnDrawGizmos();
	}

}
