using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearbyCubeTracker))]
public class Enemy : MonoWithCachedTransform
{
	public event Action<Enemy> OnRemoved;
	public event Action<Enemy> OnFinishedSpawning;

	public EnemyBody body;

	public PathFinder PathFinder { get; private set; }
	public bool IsSpawning { get; private set; }
	public float HalfEdgeSize { get; private set; }
	public EnemyType Type { get; private set; }

	protected Transform _bodyTransform;
	protected Transform BodyTransform
	{
		get
		{
			return _bodyTransform ?? (_bodyTransform = body.CachedTransform);
		}
	}

	protected NearbyCubeTracker _nearbyCubeTracker;
	protected NearbyCubeTracker CubeTracker
	{
		get
		{
			return _nearbyCubeTracker ?? (_nearbyCubeTracker = GetComponent<NearbyCubeTracker>());
		}
	}

	private AMovementStrategy _movementStrategy;
	private bool _isSetup;

	protected virtual void Awake()
	{
		IsSpawning = true;
		body.SetVisible(false);
	}

	public bool IsOverlappingAnotherCube()
	{
		return _isSetup && PathFinder.AmIOverlappingAnotherCube(HalfEdgeSize);
	}

	private void OnDestroy()
	{
		OnRemoved?.Invoke(this);
	}

	public void Setup(EnemyConfig config, float speedMultiplier = 1f)
	{
		IsSpawning = true;

		PathFinder = new PathFinder(CachedTransform, CubeTracker, config.halfBodyDiagonal);
		PickMovementStrategy(config, speedMultiplier);
		CubeTracker.UpdateTrackedAreaSize(_movementStrategy.GetMaxStepDistance());
;
		SetupBody(config);

		HalfEdgeSize = config.edgeSize / 2f;
		Type = config.type;
		_isSetup = true;

		StartCoroutine(CheckPathAndStartMovingRoutine());	
	}

	private void SetupBody(EnemyConfig config)
	{
		body.Setup(config);
		body.OnHitPointsChanged += HandleHpChanged;
		body.dealDamageOnImpact.OnImpact += HandleImpact;
		body.SetVisible(false);
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
			body.SetVisible(true);
			IsSpawning = false;
			OnFinishedSpawning?.Invoke(this);

			yield return _movementStrategy.RunRoutine();
		}
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

	public bool OverlapsAnyPositions(List<Vector3> positions, float otherCubeSize)
	{
		return PathFinder.OverlapsAnyPositions(positions, otherCubeSize);
	}

	private void OnDrawGizmos()
	{
		PathFinder?.OnDrawGizmos();
	}

}
