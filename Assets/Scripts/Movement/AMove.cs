using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AMove
{
	public const float PATH_CHECK_INTERVAL_SECONDS = 0.25f;
	protected PathFinder _pathFinder;
	protected List<Vector3> _plannedPath;

	private WaitForSeconds _pathCheckInterval = new WaitForSeconds(PATH_CHECK_INTERVAL_SECONDS);
	protected bool _lastPathFindWasSuccessful;

	protected IEnumerator WaitUntilPathFreeOrTimeOutRoutine(List<Vector3> path, float timeOut = -1f)
	{
		var elapsed = 0f;
		while (!_pathFinder.IsPathFree(path) && (timeOut < 0f || elapsed <= timeOut))
		{
			yield return _pathCheckInterval;
			elapsed += PATH_CHECK_INTERVAL_SECONDS;
		}

		if (timeOut > 0f && timeOut <= elapsed)
		{
			_lastPathFindWasSuccessful = false;
		}
		else
		{
			_pathFinder.Path = path;
			_lastPathFindWasSuccessful = true;
		}
	}
}
