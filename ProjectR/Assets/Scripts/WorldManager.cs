using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class WorldManager
{
    private static Map map;
	private static PathFinder<Vector2Int> pathFinder;

	private static List<RObject> rObjList;
	private static List<RObjectBehaviour> rObjBehaviours;

	#region events
	/// <summary>
	/// onTileChanged: 타일 변경
	/// arg1: changedPos
	/// arg2: prevTileId
	/// arg3: newTileId
	/// </summary>
	public static event Action<Vector2Int, string, string> onTileChanged;
	#endregion events

	public static void Init( Map map )
	{
		WorldManager.map = map;
		pathFinder = new PathFinder<Vector2Int>( Vector2IntExt.Get8DirectionLength );
		rObjList = new List<RObject>();
		rObjBehaviours = new List<RObjectBehaviour>();

		onTileChanged = null;
	}

	public static void Update( float dt )
	{
		foreach ( var rObj in rObjList )
		{
			rObj.Update( dt );
		}
	}

	public static void ChangeTile( Vector2Int targetPos, string tileId )
	{
		string prevTileId = map.GetTile( targetPos );
		map.SetTile( targetPos, tileId );
		onTileChanged?.Invoke( targetPos, prevTileId, tileId );
	}

	public static List<(Vector2Int, float)> FindPath( Vector2Int startPos, Vector2Int destPos, List<(Vector2Int,float)> path = null )
	{
		path = pathFinder.FindPath( map, startPos, destPos, path );

		return path;
	}

	public static Vector3 MapPosToWorldPosition( Vector2 mapPosition )
	{
		return map.MapPositionToWorldPosition( mapPosition );
	}

	public static void AddRObject( RObject rObject )
	{
		rObjList.Add( rObject );

		GameObject go = new GameObject("RObj");
		RObjectBehaviour rObjBehaviour = go.AddComponent<RObjectBehaviour>();
		rObjBehaviour.Init( rObject );

		rObjBehaviours.Add( rObjBehaviour );
	}

	public static Vector2Int WorldPositionToMapTilePos( Vector3 worldPosition )
	{
		return map.WorldPositionToMapTilePos(worldPosition);
	}

	public static void MoveTest( Vector2Int destPos )
	{
		foreach ( var rObj in rObjList )
		{
			if ( rObj is Pawn )
			{
				( rObj as Pawn ).Move( destPos );
			}
		}
	}
}
