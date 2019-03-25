using System;
using System.Collections.Generic;
using UnityEngine;

public delegate void CubeAwarenessDelegate(CubeAwareness cube, int instanceID);

public interface ICollidable
{
	event CubeAwarenessDelegate OnRemoved;

	bool OverlapsPosition(Vector3 position, float cubeSize);
	bool OverlapsAnyPosition(IEnumerable<Vector3> positions, float cubeSize, bool log = false);
}