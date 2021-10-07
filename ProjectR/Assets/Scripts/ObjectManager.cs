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

    public ObjectManager()
    {
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
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        rObject.RefreshRegion(true);
        GetOrMakeObjectDict(rObject.LocalRegion).Add(rObject.IndexKey, rObject);
        onCreateObjectEvent?.Invoke(rObject);
        iteratorList.Add(rObject);
        Debug.Log($"CreateObject: {stopwatch.ElapsedTicks}");
    }

    public void DestroyObject(RObject rObject)
    {
        destroyList.Add(rObject);
    }

    public void CreateItem(Vector2Int pos, ItemDataDescriptor itemDesc, int amount)
    {
        Dictionary<Vector2Int, ItemObject> existItems = new Dictionary<Vector2Int, ItemObject>();
        foreach (var objectDict in objects.Values)
        {
            foreach (var rObj in objectDict)
            {
                ItemObject itemObj = rObj.Value as ItemObject;
                if (itemObj != null)
                {
                    existItems.Add(itemObj.MapTilePosition, itemObj);
                }
            }
        }

        if (existItems.ContainsKey(pos) == false)
        {
            ItemObject newItem = new ItemObject(itemDesc, Mathf.Min(amount, itemDesc.StackAmount));
            newItem.MapTilePosition = pos;
            CreateObject(newItem);
            amount -= itemDesc.StackAmount;
        }

        if (amount > 0)
        {
            int searchAmount = 100;
            //1/3/5/7/9
            //1/2/3/4/5
            for (int i = 2; i <= searchAmount; ++i)
            {
                int sqrtCeil = Mathf.CeilToInt(Mathf.Sqrt(i));
                int squareLength = Mathf.RoundToInt(((sqrtCeil / 2) + 0.5f) * 2);
                int prevSquareLength = squareLength - 2;
                int tt = (squareLength - 1) / 2;

                Vector2Int searchPos;

                int insideIndex = i - prevSquareLength * prevSquareLength;

                int side = Mathf.CeilToInt((float)insideIndex / (squareLength - 1)) - 1;
                int squareIndex = insideIndex - side * (squareLength - 1) - 1;

                if (side == 0)
                {
                    Vector2Int startPos = pos + new Vector2Int(tt, -tt + 1);
                    searchPos = startPos + new Vector2Int(0, squareIndex);
                }
                else if (side == 1)
                {
                    Vector2Int startPos = pos + new Vector2Int(tt - 1, tt);
                    searchPos = startPos + new Vector2Int(-squareIndex, 0);
                }
                else if (side == 2)
                {
                    Vector2Int startPos = pos + new Vector2Int(-tt, tt - 1);
                    searchPos = startPos + new Vector2Int(0, -squareIndex);
                }
                else
                {
                    Vector2Int startPos = pos + new Vector2Int(-tt + 1, -tt);
                    searchPos = startPos + new Vector2Int(squareIndex, 0);
                }

                if (existItems.TryGetValue(searchPos, out var itemObj) == true)
                {
                    if (itemObj.Desc == itemDesc && itemObj.Amount < itemDesc.StackAmount)
                    {
                        int add = Mathf.Min(amount, itemDesc.StackAmount - itemObj.Amount);
                        itemObj.Amount += add;
                        amount -= add;
                    }
                }
                else if (GameManager.Instance.WorldMap.GetTileMovableWeight(searchPos) > 0)
                {
                    ItemObject newItem = new ItemObject(itemDesc, Mathf.Min(amount, itemDesc.StackAmount));
                    newItem.MapTilePosition = searchPos;
                    CreateObject(newItem);
                    amount -= itemDesc.StackAmount;
                }

                if (amount <= 0)
                    break;
            }
        }
    }

    private void DestroyAllInList()
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        foreach (var rObj in destroyList)
        {
            objects[rObj.LocalRegion]?.Remove(rObj.IndexKey);
            onDestroyObjectEvent?.Invoke(rObj);

            int removeIndex = iteratorList.BinarySearch(rObj, RObjectUIDComparer.Comparer);
            iteratorList.RemoveAt(removeIndex);

            rObj.Destroy();
        }

        destroyList.Clear();
        Debug.Log($"Destroy: {stopwatch.ElapsedTicks}");
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