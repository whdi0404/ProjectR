//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.Tilemaps;

//using Random = UnityEngine.Random;
//using Object = UnityEngine.Object;

//public class World
//{
//	private const string mapPrefabPath = "World/Map"; 

//    private Map map;

//	private List<RObject> rObjList;

//    private Vector2Int sectionSize = new Vector2Int(256, 256);
//    private Dictionary<Vector2Int, List<RObject>> objSection;

//	private List<RObjectBehaviour> usingRObjBehaviours;
//	private List<RObjectBehaviour> rObjBehavioursPool;

//	#region events
//	/// <summary>
//	/// onTileChanged: 타일 변경
//	/// arg1: changedPos
//	/// arg2: prevTileId
//	/// arg3: newTileId
//	/// </summary>
//	public event Action<Vector2Int, string, string> onTileChanged;
//	#endregion events

//	public World()
//	{
//		GameObject prefab = Resources.Load<GameObject>(mapPrefabPath);
//		if (prefab == null)
//		{
//			Debug.LogError($"MapPrefab is not exist, Path: {mapPrefabPath}");
//			return;
//		}

//		GameObject mapObj = Object.Instantiate(prefab);
//		Map map = mapObj.GetComponent<Map>();

//		this.map = map;
//		rObjList = new List<RObject>();
//		usingRObjBehaviours = new List<RObjectBehaviour>();
//		rObjBehavioursPool = new List<RObjectBehaviour>();

//		objSection = new Dictionary<Vector2Int, List<RObject>>();

//		onTileChanged = null;

//		//var plantenableTileArray = map.PlantEnableTiles.ToList();

//		//int treeInitAmount = 1;// map.PlantEnableTiles.Count / 10;
//		//for (int i = 0; i < treeInitAmount; ++i)
//		//{
//		//	int randIdx = Random.Range(0, plantenableTileArray.Count);

//		//	plantenableTileArray.RemoveAt(randIdx);
//  //          DataContainer treePreset = new DataContainer() 
//		//	{ 
//		//		{ "plantId","tree" },
//		//		{ "breedableAge", 5f },
//		//		{ "breedPeriod", 1f },
//		//		{ "breedAmount", 1 },
//		//		{ "age", Random.Range(0, 20f) }
//		//	};
//  //          Plant tree = new Plant(treePreset);
//		//	tree.MapTilePosition = plantenableTileArray[randIdx];
//		//	AddRObject(tree);
//		//}
//    }

//	public  void Update( float dt )
//	{
//		for (int i = 0; i < rObjList.Count; ++i)
//		{
//			RObject rObj = rObjList[i];
//			rObj.Update(dt);
//		}
//	}

//	//private void RObjectBehaviourUpdate()
//	//{
//	//	Camera currentCam = Camera.main;

//	//	Bounds camBound = new Bounds(currentCam.transform.position, new Vector3(currentCam.orthographicSize * currentCam.r, currentCam.orthographicSize * 2 ));
//	//	.Intersects(new Bounds());
//	//	.x;

//	//	RObjectBehaviour rObjBehaviour = null;
//	//	if (rObjBehavioursPool.Count > 0)
//	//	{
//	//		rObjBehaviour = rObjBehavioursPool[rObjBehavioursPool.Count - 1];
//	//		rObjBehavioursPool.RemoveAt(rObjBehavioursPool.Count - 1);
//	//	}
//	//	else
//	//	{
//	//		GameObject go = new GameObject();
//	//		rObjBehaviour = go.AddComponent<RObjectBehaviour>();
//	//	}

//	//	rObjBehaviour.Init(rObject);

//	//	usingRObjBehaviours.Add(rObjBehaviour);
//	//}

//	public  void UpdateSection()
//	{
//	}

//	public void ChangeTile( Vector2Int targetPos, string tileId )
//	{
//		string prevTileId = map.GetTile( targetPos );
//		map.SetTile( targetPos, tileId );
//		onTileChanged?.Invoke( targetPos, prevTileId, tileId );
//	}

//	public  Vector3 MapPosToWorldPosition( Vector2 mapPosition )
//	{
//		return map.MapPositionToWorldPosition( mapPosition );
//	}

//	public  Vector2Int WorldPositionToMapTilePos(Vector3 worldPosition)
//	{
//		return map.WorldPositionToMapTilePos(worldPosition);
//	}

//	public  void AddRObject( RObject rObject )
//	{
//		rObjList.Add(rObject);

//		GameObject go = new GameObject();
//		RObjectBehaviour rObjBehaviour = go.AddComponent<RObjectBehaviour>();
//		rObjBehaviour.Init(rObject);
//		usingRObjBehaviours.Add(rObjBehaviour);
//    }

//    public  List<RObject> IsExistPawn(Vector2Int mapPosition)
//	{
//		List<RObject> rObjs = new List<RObject>();
//		foreach (var rObj in rObjList)
//		{
//			if (rObj.MapTilePosition == mapPosition && rObj is Pawn)
//				rObjs.Add(rObj);
//		}

//		return rObjs;
//	}

//	public  bool IsPlantable(Vector2Int mapPosition)
//    {
//        if (map.IsPlantEnableTile(mapPosition) == false)
//            return false;
//		if (IsExistPlant(mapPosition) != null)
//			return false;

//		return true;
//	}
//}
