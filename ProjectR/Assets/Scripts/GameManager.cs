using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct NearNode_RObj<T> : IComparable<NearNode_RObj<T>> where T : RObject
{
    public T rObj;
    public float distance;

    public int CompareTo(NearNode_RObj<T> other)
    {
        return distance.CompareTo(other.distance);
    }
}

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GameManager : SingletonBehaviour<GameManager>
{
    public ulong UniqueIdCounter;

    public WorldMap WorldMap { get; private set; }

    public ObjectManagingSystem ObjectManagingSystem { get; private set; }
    public SmartDictionary<ulong, RObject> RObjList { get; private set; } = new SmartDictionary<ulong, RObject>();

    private List<ulong> destroyList = new List<ulong>();

    private Dictionary<AITagSubject, AITagSystem> tagSystemDict = new Dictionary<AITagSubject, AITagSystem>();

    protected override void Start()
    {
        UniqueIdCounter = 1;
        WorldMap = new GameObject("WorldMap").AddComponent<WorldMap>();

        GOPoolManager.Instance.Init("RObj", new GameObject("RObject", typeof(RObjectBehaviour)));

        DeveloperTool.Instance.On = true;
    }

    private void Update()
    {
        foreach (var uniqueId in destroyList)
        {
            RObjList[uniqueId]?.Destroy();
            RObjList.Remove(uniqueId);
        }

        foreach (var rObj in RObjList.Values)
            rObj.Update(Time.deltaTime);
    }

    private void LateUpdate()
    {
        foreach (var rObj in RObjList.Values)
            rObj.VisuaiUpdate();
    }

    public void CreateRObject(RObject rObj)
    {
        var uniqueId = UniqueIdCounter++;
        rObj.UniqueId = uniqueId;

        RObjList[uniqueId] = rObj;
    }

    public void DestroyRObject(RObject rObj)
    {
        destroyList.Add(rObj.UniqueId);
    }

    public void DestroyRObject(ulong uniqueId)
    {
        destroyList.Add(uniqueId);
    }

    public void CreateItem(Vector2Int pos, ItemDataDescriptor itemDesc, int amount)
    {
        Dictionary<Vector2Int, ItemObject> existItems = new Dictionary<Vector2Int, ItemObject>();

        foreach (var rObj in RObjList.Values)
        {
            ItemObject itemObj = rObj as ItemObject;
            if (itemObj != null)
            {
                existItems.Add(itemObj.MapTilePosition, itemObj);
            }
        }

        if (existItems.ContainsKey(pos) == false)
        {
            ItemObject newItem = new ItemObject(itemDesc, Mathf.Min(amount, itemDesc.StackAmount));
            newItem.MapTilePosition = pos;
            CreateRObject(newItem);
            amount -= itemDesc.StackAmount;
        }

        if(amount > 0)
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
                else if(WorldMap.GetTileMovableWeight(searchPos) > 0)
                {
                    ItemObject newItem = new ItemObject(itemDesc, Mathf.Min(amount, itemDesc.StackAmount));
                    newItem.MapTilePosition = searchPos;
                    CreateRObject(newItem);
                    amount -= itemDesc.StackAmount;
                }

                if (amount <= 0)
                    break;
            }
        }
    }

    public AITagSystem GetAITagSystem(AITagSubject subject)
    {
        if (tagSystemDict.TryGetValue(subject, out AITagSystem aiTagSystem) == false)
            tagSystemDict.Add(subject, aiTagSystem = new AITagSystem());

        return aiTagSystem;
    }

    public IEnumerable<T> GetRObjectsFromType<T>() where T : RObject
    {
        foreach (var rObj in GameManager.Instance.RObjList.Values)
        {
            T planObj = rObj as T;
            if (planObj == null)
                continue;

            yield return planObj;
        }
    }

    public IEnumerable<T> GetNearestRObjectsFromType<T>(Vector2Int pos, Predicate<RObject> predicate = null) where T : RObject
    {
        PriorityQueue<NearNode_RObj<T>> queue = new PriorityQueue<NearNode_RObj<T>>();
        foreach (var rObj in GetRObjectsFromType<T>())
        {
            if (predicate != null && predicate(rObj) == false)
                continue;

            NearNode_RObj<T> nearNode = new NearNode_RObj<T>
            {
                rObj = rObj,
                distance = Vector2Int.Distance(pos, rObj.MapTilePosition)
            };

            queue.Enqueue(nearNode);
        }

        while (queue.Count > 0)
            yield return queue.Dequeue().rObj;
    }

    public bool FindPath(Vector2Int start, Vector2Int dest, ref List<(Vector2Int, float)> path, int maxSearch = 10000)
    {
        return WorldMap.PathFinder.FindPath(WorldMap, start, dest, ref path, maxSearch);
    }
    public bool IsReachable(Vector2Int start, Vector2Int dest)
    {
        return WorldMap.RegionSystem.IsReachable(start, dest);
    }
}