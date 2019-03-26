using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearbyCubeTracker))]
public class Enemy : MonoWithCachedTransform
{
	public event Action<Enemy> OnRemoved;

	public Transform mesh;
	public PathFinder PathFinder { get; private set; }

	private NearbyCubeTracker _nearbyCubeTracker;
	private NearbyCubeTracker CubeTracker
	{
		get
		{
			return _nearbyCubeTracker ?? (_nearbyCubeTracker = GetComponent<NearbyCubeTracker>());
		}
	}

	private AMovementStrategy _movementStrategy;

	public void Setup(EnemyConfig config, float speedMultiplier = 1f)
	{
		PathFinder = new PathFinder(CachedTransform, CubeTracker, config.halfBodyDiagonal);
		PickMovementStrategy(config, speedMultiplier);
		CubeTracker.UpdateTrackedAreaSize(_movementStrategy.GetMaxStepDistance());
		UpdateVisuals(config);

		StartCoroutine(_movementStrategy.RunRoutine());
	}

	private void UpdateVisuals(EnemyConfig config)
	{
		mesh.localScale = new Vector3(config.edgeSize, config.edgeSize, config.edgeSize);
	}

	private void PickMovementStrategy(EnemyConfig config, float speedMultiplier = 1f)
	{
		//TODO
		_movementStrategy = new RollStrategy(CachedTransform, mesh, PathFinder, config.edgeSize, config.speedUnitsPerSecond * speedMultiplier);
	}

	public bool OverlapsAnyPositions(IEnumerable<Vector3> positions, float otherCubeSize)
	{
		return PathFinder.OverlapsAnyPositions(positions, otherCubeSize);
	}

	private void OnDrawGizmos()
	{
		PathFinder.OnDrawGizmos();
	}

}
