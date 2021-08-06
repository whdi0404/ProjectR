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

public enum AITagSubject
{
    PickupItem,
    Build,
    Attack,
}

public class AITagSystem
{

    class Tagging
    {
        Pawn ai;
        RObject tagObject;
    }

    private SmartDictionary<Pawn, List<RObject>> tag = new SmartDictionary<Pawn, List<RObject>>();
    private SmartDictionary<RObject, List<Pawn>> tagged = new SmartDictionary<RObject, List<Pawn>>();

    public void Tag(Pawn pawn, RObject rObj)
    {
        if (tag[pawn] == null)
            tag[pawn] = new List<RObject>();
        tag[pawn].Add(rObj);

        if (tagged[rObj] == null)
            tagged[rObj] = new List<Pawn>();
        tagged[rObj].Add(pawn);
    }

    public void UnTag(Pawn pawn, RObject rObj)
    {
        if (tag[pawn]?.Remove(rObj) == true)
        {
            //Event
        }
        if (tagged[rObj]?.Remove(pawn) == true)
        {
            //Event
        }
    }

    public void UnTagAll(RObject rObj)
    {
        var taggedList = tagged[rObj];
        if (taggedList == null)
            return;
        foreach (var pawn in taggedList)
        {
            UnTag(pawn, rObj);
        }
    }

    public void UntagAllTagOfPawn(Pawn pawn)
    {
        var tagList = tag[pawn];
        if (tagList == null)
            return;
        foreach (var rObj in tagList)
        {
            UnTag(pawn, rObj);
        }
    }

    public bool IsTagged(RObject rObj)
    {
        return tagged[rObj]?.Count > 0;
    }

    public List<RObject> GetTaggedObjectsOfPawn(Pawn pawn) 
    { 
        return tag[pawn];
    }
}

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GameManager : SingletonBehaviour<GameManager>
{
    public ulong UniqueIdCounter;

    public WorldMap WorldMap { get; private set; }
    public SmartDictionary<ulong, RObject> RObjList { get; private set; } = new SmartDictionary<ulong, RObject>();

    private Dictionary<AITagSubject, AITagSystem> tagSystemDict = new Dictionary<AITagSubject, AITagSystem>();

    private PathFinder<Vector2Int> pathFinder = new PathFinder<Vector2Int>((a, b) => VectorExt.Get8DirectionLength(a,b));

    protected override void Start()
    {
        UniqueIdCounter = 1;
        WorldMap = new GameObject("WorldMap").AddComponent<WorldMap>();

        GOPoolManager.Instance.Init("RObj", new GameObject("RObject", typeof(RObjectBehaviour)));
    }

    private void Update()
    {
        foreach (var rObj in RObjList.Values)
            rObj.Update(Time.deltaTime);
    }

    public void CreateRObject(RObject rObj)
    {
        var uniqueId = UniqueIdCounter++;
        rObj.UniqueId = uniqueId;

        RObjList[uniqueId] = rObj;
    }

    public void DestroyRObject(RObject rObj)
    {
        rObj.Destroy();
        RObjList.Remove(rObj.UniqueId);
    }

    public void DestroyRObject(ulong uniqueId)
    {
        RObjList[uniqueId]?.Destroy();
        RObjList.Remove(uniqueId);
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

    public IEnumerable<T> GetNearestRObjectsFromType<T>(Vector2Int pos) where T : RObject
    {
        PriorityQueue<NearNode_RObj<T>> queue = new PriorityQueue<NearNode_RObj<T>>();
        foreach (var rObj in GetRObjectsFromType<T>())
        {
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
        return pathFinder.FindPath(WorldMap, start, dest, ref path, maxSearch);
    }
}