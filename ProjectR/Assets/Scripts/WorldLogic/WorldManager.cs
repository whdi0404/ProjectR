using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

using Random = UnityEngine.Random;

public static class WorldManager
{
    private static Map map;

	private static PathFinder<Vector2Int> pathFinder;

	private static List<RObject> rObjList;

    private static Vector2Int sectionSize = new Vector2Int(256, 256);
    private static Dictionary<Vector2Int, List<RObject>> objSection;

	private static List<RObjectBehaviour> usingRObjBehaviours;
	private static List<RObjectBehaviour> rObjBehavioursPool;

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
		pathFinder = new PathFinder<Vector2Int>( VectorExt.Get8DirectionLength );
		rObjList = new List<RObject>();
		usingRObjBehaviours = new List<RObjectBehaviour>();
		rObjBehavioursPool = new List<RObjectBehaviour>();

		objSection = new Dictionary<Vector2Int, List<RObject>>();

		onTileChanged = null;

		var plantenableTileArray = map.PlantEnableTiles.ToList();

		int treeInitAmount = 1;// map.PlantEnableTiles.Count / 10;
		for (int i = 0; i < treeInitAmount; ++i)
		{
			int randIdx = Random.Range(0, plantenableTileArray.Count);

			plantenableTileArray.RemoveAt(randIdx);
            DataContainer treePreset = new DataContainer() 
			{ 
				{ "plantId","tree" },
				{ "breedableAge", 5f },
				{ "breedPeriod", 1f },
				{ "breedAmount", 1 },
				{ "age", Random.Range(0, 20f) }
			};
            Plant tree = new Plant(treePreset);
			tree.MapTilePosition = plantenableTileArray[randIdx];
			AddRObject(tree);
		}
    }

	public static void Update( float dt )
	{
		for (int i = 0; i < rObjList.Count; ++i)
		{
			RObject rObj = rObjList[i];
			rObj.Update(dt);
		}
	}

	//private void RObjectBehaviourUpdate()
	//{
	//	Camera currentCam = Camera.main;

	//	Bounds camBound = new Bounds(currentCam.transform.position, new Vector3(currentCam.orthographicSize * currentCam.r, currentCam.orthographicSize * 2 ));
	//	.Intersects(new Bounds());
	//	.x;

	//	RObjectBehaviour rObjBehaviour = null;
	//	if (rObjBehavioursPool.Count > 0)
	//	{
	//		rObjBehaviour = rObjBehavioursPool[rObjBehavioursPool.Count - 1];
	//		rObjBehavioursPool.RemoveAt(rObjBehavioursPool.Count - 1);
	//	}
	//	else
	//	{
	//		GameObject go = new GameObject();
	//		rObjBehaviour = go.AddComponent<RObjectBehaviour>();
	//	}

	//	rObjBehaviour.Init(rObject);

	//	usingRObjBehaviours.Add(rObjBehaviour);
	//}

	public static void UpdateSection()
	{
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

	public static Vector2Int WorldPositionToMapTilePos(Vector3 worldPosition)
	{
		return map.WorldPositionToMapTilePos(worldPosition);
	}

	public static void AddRObject( RObject rObject )
	{
		rObjList.Add(rObject);

		GameObject go = new GameObject();
		RObjectBehaviour rObjBehaviour = go.AddComponent<RObjectBehaviour>();
		rObjBehaviour.Init(rObject);
		usingRObjBehaviours.Add(rObjBehaviour);
    }

    public static List<RObject> IsExistPawn(Vector2Int mapPosition)
	{
		List<RObject> rObjs = new List<RObject>();
		foreach (var rObj in rObjList)
		{
			if (rObj.MapTilePosition == mapPosition && rObj is Pawn)
				rObjs.Add(rObj);
		}

		return rObjs;
	}
	public static RObject IsExistPlant(Vector2Int mapPosition)
	{
		foreach (var rObj in rObjList)
		{
			if (rObj.MapTilePosition == mapPosition && rObj is Plant)
				return rObj;
		}

		return null;
	}

	public static bool IsPlantable(Vector2Int mapPosition)
    {
        if (map.IsPlantEnableTile(mapPosition) == false)
            return false;
		if (IsExistPlant(mapPosition) != null)
			return false;

		return true;
	}
}
