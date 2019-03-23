using System.Collections.Generic;
using UnityEngine;

public interface ICollidable
{
	bool OverlapsPosition(Vector3 position, float cubeSize);
	bool OverlapsAnyPosition(IEnumerable<Vector3> positions, float cubeSize);
}