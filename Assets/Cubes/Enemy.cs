﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NearbyCubeTracker))]
public class Enemy : MonoWithCachedTransform
{
	public event Action<Enemy> OnRemoved;

	public Transform mesh;
	public Renderer meshRenderer;
	public PathFinder PathFinder { get; private set; }

	public bool IsSpawning { get; private set; }

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
		meshRenderer.enabled = false;
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
		UpdateVisuals(config);

		_isSetup = true;
		IsSpawning = true;
		StartCoroutine(CheckPathAndStartMovingRoutine());	
	}

	//TODO
	private void LateUpdate()
	{
		if (_isSetup && !IsSpawning && PathFinder.AmIOverlappingAnotherCube(mesh.localScale.x / 2f))
		{
			Debug.LogError(gameObject.name);
			Debug.Break();
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
			meshRenderer.enabled = true;
			IsSpawning = false;
			yield return _movementStrategy.RunRoutine();
		}
	}

	private void UpdateVisuals(EnemyConfig config)
	{
		mesh.localScale = new Vector3(config.edgeSize, config.edgeSize, config.edgeSize);
	}

	private void PickMovementStrategy(EnemyConfig config, float speedMultiplier = 1f)
	{
		//TODO
		switch (config.type)
		{
			case EnemyType.Simple: 
			case EnemyType.Zigzag:
			case EnemyType.Titan:
				_movementStrategy = new RollStrategy(CachedTransform, mesh, PathFinder, config, speedMultiplier); 
				break;

			case EnemyType.Jumper:
				_movementStrategy = new JumpStrategy(CachedTransform, mesh, PathFinder, config, speedMultiplier);
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
