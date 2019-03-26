using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearbyCubeTracker))]
public class Enemy : MonoWithCachedTransform
{
	public event Action<Enemy> OnRemoved;

	public PathFinder PathFinder { get; private set; }

	private NearbyCubeTracker _nearbyCubeTracker;
	private NearbyCubeTracker CubeTracker
	{
		get
		{
			return _nearbyCubeTracker ?? (_nearbyCubeTracker = GetComponent<NearbyCubeTracker>());
		}
	}

	public void Setup(EnemyConfig config)
	{
		PathFinder = new PathFinder(CachedTransform, CubeTracker, config.halfBodyDiagonal);
		// pick movement strategy based on config
		// from strategy => get step distance
		// with step distance => update Cube tracker's tracked area
	}

	public bool OverlapsAnyPositions(IEnumerable<Vector3> positions, float otherCubeSize)
	{
		return PathFinder.OverlapsAnyPositions(positions, otherCubeSize);
	}
}
