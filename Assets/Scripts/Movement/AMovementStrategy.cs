using System;
using System.Collections;
using UnityEngine;

public abstract class AMovementStrategy
{
	public event Action OnStepFinished;

	protected PathFinder _pathFinder;
	protected Transform _cachedTransform;
	protected Transform _meshToRotate;
	protected float _edgeSize;
	protected float _rollAnglePerUpdate;

	public AMovementStrategy(Transform cachedTransform, Transform meshToRotate, PathFinder pathFinder, 
							 float edgeSize, float cubeSpeedUnitsPerSecond)
	{
		_cachedTransform = cachedTransform;
		_meshToRotate = meshToRotate;
		_pathFinder = pathFinder;
		_edgeSize = edgeSize;

		_rollAnglePerUpdate = CalculateRollAnglePerUpdate(edgeSize, cubeSpeedUnitsPerSecond);
	}

	public abstract IEnumerator RunRoutine();
	public abstract float GetMaxStepDistance();

	protected void StepFinished()
	{
		OnStepFinished?.Invoke();
	}	

	private float CalculateRollAnglePerUpdate(float edgeSize, float cubeSpeedUnitsPerSecond)
	{
		return cubeSpeedUnitsPerSecond / edgeSize * 90f * Time.fixedDeltaTime;
	}
}
