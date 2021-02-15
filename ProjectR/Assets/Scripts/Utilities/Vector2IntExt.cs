using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector2IntExt
{
	public static float Get8DirectionLength( this Vector2Int vec )
	{
		int vecXAbs = Mathf.Abs( vec.x );
		int vecYAbs = Mathf.Abs( vec.y );
		(int, int) minMaxElem = vecXAbs > vecYAbs ? (vecYAbs, vecXAbs) : (vecXAbs, vecYAbs);
		float len = minMaxElem.Item1 * 0.41421356237f + minMaxElem.Item2;
		return len;
	}

	public static float Get8DirectionLength( Vector2Int v1, Vector2Int v2 )
	{
		return ( v1 - v2 ).Get8DirectionLength();
	}
}