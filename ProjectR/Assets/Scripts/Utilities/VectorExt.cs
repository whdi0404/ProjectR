using UnityEngine;

public static class VectorExt
{
	public static float Get8DirectionLength(this Vector2Int vec)
	{
		int vecXAbs = Mathf.Abs(vec.x);
		int vecYAbs = Mathf.Abs(vec.y);
		(int, int) minMaxElem = vecXAbs > vecYAbs ? (vecYAbs, vecXAbs) : (vecXAbs, vecYAbs);
		float len = minMaxElem.Item1 * 0.41421356237f + minMaxElem.Item2;
		return len;
	}

	public static float Get8DirectionLength(Vector2Int v1, Vector2Int v2)
	{
		return (v1 - v2).Get8DirectionLength();
	}

	public static Vector2 ToVec2Float(this Vector2Int v)
	{
		return new Vector2(v.x, v.y);
	}

	public static Vector2Int ToVec2Int(this Vector2 v)
	{
		return new Vector2Int((int)v.x, (int)v.y);
	}

	public static bool Intersects(this BoundsInt a, BoundsInt b)
	{
		Bounds boundsVec = new Bounds(a.center, a.size);
		return boundsVec.Intersects(new Bounds(b.center, b.size));
	}

	public static bool TileContains(this BoundsInt bounds, Vector2Int vector)
	{
		return bounds.xMin <= vector.x && bounds.xMax > vector.x &&
		bounds.yMin <= vector.y && bounds.yMax > vector.y;
	}
}