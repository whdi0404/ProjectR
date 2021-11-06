using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public interface IObjectManagerListener
{
    public void OnCreateObject( RObject rObject );
    public void OnDestroyObject( RObject rObject );
}

public class ObjectManager : IRegionListener
{
    private event Action<RObject> onCreateObjectEvent;
    private event Action<RObject> onDestroyObjectEvent;

    private SmartDictionary<LocalRegion, HierarchyDictionary<string, RObject>> objects = new SmartDictionary<LocalRegion, HierarchyDictionary<string, RObject>>();
    private List<RObject> destroyList = new List<RObject>();
    private List<RObject> iteratorList = new List<RObject>();

    public ItemSystem ItemSystem { get; private set; }

    public ObjectManager()
    {
        ItemSystem = new ItemSystem();
        AddListener(ItemSystem);
    }

    public void AddListener(IObjectManagerListener listener)
    {
        onCreateObjectEvent += listener.OnCreateObject;
        onDestroyObjectEvent += listener.OnDestroyObject;
    }

    public void RemoveListener(IObjectManagerListener listener)
    {
        onCreateObjectEvent -= listener.OnCreateObject;
        onDestroyObjectEvent -= listener.OnDestroyObject;
    }

    public void OnRegionChange(List<LocalRegion> removedLocalRegions, List<LocalRegion> newLocalRegions)
    {
        foreach (var removedRegion in removedLocalRegions)
            objects.Remove(removedRegion);

        foreach (var newLocalRegion in newLocalRegions)
            objects.Add(newLocalRegion, new HierarchyDictionary<string, RObject>());

        foreach (var removedRegion in removedLocalRegions)
        {
            if (objects.TryGetValue(removedRegion, out var regionObjects) == true)
            {
                foreach (var obj in regionObjects)
                    obj.Value.RefreshRegion();
            }
        }
            
    }

    private HierarchyDictionary<string, RObject> GetOrMakeObjectDict(LocalRegion region)
    {
        if (objects.TryGetValue(region, out var objDict) == false)
        {
            objDict = new HierarchyDictionary<string, RObject>();
            objects.Add(region, objDict);
        }

        return objDict;
    }

    public class RObjectUIDComparer : IComparer<RObject>
    {
        public static readonly RObjectUIDComparer Comparer = new RObjectUIDComparer();

        public int Compare(RObject x, RObject y)
        {
            return x.UniqueId.CompareTo(y.UniqueId);
        }
    }

    public void OnObjectRefreshRegion(RObject rObject, LocalRegion prevRegion)
    {
        if (prevRegion == rObject.LocalRegion)
            return;

        if (prevRegion != null)
            objects[prevRegion]?.Remove(rObject.IndexKey);
        GetOrMakeObjectDict(rObject.LocalRegion).Add(rObject.IndexKey, rObject);
    }

    public void CreateObject(RObject rObject)
    {
        rObject.Init();
        GetOrMakeObjectDict(rObject.LocalRegion).Add(rObject.IndexKey, rObject);
        onCreateObjectEvent?.Invoke(rObject);
        iteratorList.Add(rObject);
    }

    public void DestroyObject(RObject rObject)
    {
        if(rObject.HasUniqueId == true)
            destroyList.Add(rObject);
    }

    private void DestroyAllInList()
    {
        foreach (var rObj in destroyList)
        {
            if (rObj.HasUniqueId == false)
                Debug.LogWarning("uid없는 오브젝트 삭제");

            if (objects[rObj.LocalRegion]?.Remove(rObj.IndexKey) == true)
            {
                int idx = iteratorList.BinarySearch(rObj, RObjectUIDComparer.Comparer);
                iteratorList.RemoveAt(idx);
                rObj.Destroy();
                onDestroyObjectEvent?.Invoke(rObj);
            }
            else
            {
                Debug.LogWarning($"오브젝트 삭제 실패(uid: {rObj.UniqueId}");
            }
        }

        destroyList.Clear();
    }

    public SmartDictionary<LocalRegion, HierarchyDictionary<string, RObject>> GetAllObjects()
    {
        return objects;
    }

    public List<RObject> GetObjectsFromRegion(LocalRegion region)
    {
        List<RObject> selectedObjects = new List<RObject>();
        foreach (var obj in objects[region])
            selectedObjects.Add(obj.Value);

        return selectedObjects;
    }

    public SmartDictionary<LocalRegion, List<RObject>> GetObjectsFromIndexId(string indexId)
    {
        SmartDictionary<LocalRegion, List<RObject>> selectedObjects = new SmartDictionary<LocalRegion, List<RObject>>();
        foreach (var regionObjs in objects)
        {
            if (regionObjs.Value.TryGetValues(indexId.Split('/'), out var objList) == true)
                selectedObjects.Add(regionObjs.Key, objList);
        }

        return selectedObjects;
    }

    public List<RObject> GetObjects(LocalRegion region, string indexId)
    {
        if (objects.TryGetValue(region, out var regionObjs) == true)
        {
            regionObjs.TryGetValues(indexId.Split('/'), out List<RObject> objList);
            return objList;
        }

        return new List<RObject>();
    }

    public RObject GetObjectFromUniqueId(ulong uniqueId)
    {
        foreach (var regionObjects in objects.Values)
        {
            if (regionObjects.TryGetLeafValue(uniqueId.ToString(), out RObject rObj) == true)
                return rObj;
        }

        return null;
    }

    public IEnumerable<RObject> GetNearestObjectFromIndexId(string indexId, Vector2Int pos, Predicate<RObject> predicate = null)
    {
        RegionSystem regionSystem = GameManager.Instance.WorldMap.RegionSystem;

        if (regionSystem.GetRegionFromTilePos(pos, out LocalRegion region) == false)
            yield break;

        PriorityQueue<NearNode<RObject>> pq = new PriorityQueue<NearNode<RObject>>();
        foreach (var regionObjList in GetObjectsFromIndexId(indexId))
        {
            if (regionSystem.IsReachable(region, regionObjList.Key) == false)
                continue;

            foreach (var rObj in regionObjList.Value)
            {
                if (predicate != null && predicate(rObj) == false)
                    continue;

                NearNode<RObject> nearNode = new NearNode<RObject>
                {
                    position = rObj,
                    distance = Vector2Int.Distance(pos, rObj.MapTilePosition)
                };

                pq.Enqueue(nearNode);
            }
        }

        while (pq.Count > 0)
            yield return pq.Dequeue().position;
    }

    public IEnumerable<TRObj> GetNearestObjectFromIndexId<TRObj>(string indexId, Vector2Int pos, Predicate<TRObj> predicate = null) where TRObj : RObject
    {
        RegionSystem regionSystem = GameManager.Instance.WorldMap.RegionSystem;

        if (regionSystem.GetRegionFromTilePos(pos, out LocalRegion region) == false)
            yield break;

        PriorityQueue<NearNode<TRObj>> pq = new PriorityQueue<NearNode<TRObj>>();
        foreach (var regionObjList in GetObjectsFromIndexId(indexId))
        {
            if (regionSystem.IsReachable(region, regionObjList.Key) == false)
                continue;

            foreach (var rObj in regionObjList.Value)
            {
                if (rObj is TRObj == false)
                    continue;

                TRObj tRObj = rObj as TRObj;

                if (predicate != null && predicate(tRObj) == false)
                    continue;

                NearNode<TRObj> nearNode = new NearNode<TRObj>
                {
                    position = tRObj,
                    distance = Vector2Int.Distance(pos, rObj.MapTilePosition)
                };

                pq.Enqueue(nearNode);
            }
        }

        while (pq.Count > 0)
            yield return pq.Dequeue().position;
    }

    public void Update()
    {
        if (destroyList.Count > 0)
            DestroyAllInList();

        foreach (var rObj in iteratorList)
        {
            rObj.Update(Time.deltaTime);
        }
            
    }

    public void LateUpdate()
    {
        foreach (var rObj in iteratorList)
        {
            rObj.UpdateBehaviourVisible();
        }
    }
}