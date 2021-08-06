using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
using System.Linq;
using System;

//밥먹기

//잠자기

//건설하기
public class Build : ActionTask
{
    public Build()
    {
    }

    public override IEnumerable<State> Run(Pawn pawn)
    {
        var taggedList = GameManager.Instance.GetAITagSystem(AITagSubject.Build).GetTaggedObjectsOfPawn(pawn);
        if (taggedList == null || taggedList.Count == 0)
            yield return State.Failed;

        PlanObject planObject = taggedList[0] as PlanObject;

        if (planObject?.IsAdjeceny(pawn) == false)
            yield return State.Failed;

        while (planObject.RemainWorkload > 0)
        {
            planObject.Work(Time.deltaTime * 1);
            yield return State.Running;
        }

        yield return State.Complete;
    }
}


//만들기

//재료 옮기기
public class TagBuildTransport : ActionTask
{
    private SmartDictionary<ItemDataDescriptor, List<ItemObject>> nearestItemList;
    private SmartDictionary<ItemDataDescriptor, int> itemAmount;
    private SmartDictionary<ItemDataDescriptor, int> inventoryItem;
    private float remainWeight;

    public TagBuildTransport()
    { 
    }

    protected override bool Run(Pawn pawn)
    {
        nearestItemList = new SmartDictionary<ItemDataDescriptor, List<ItemObject>>();
        inventoryItem = new SmartDictionary<ItemDataDescriptor, int>(pawn.Inventory.ItemDict);
        remainWeight = pawn.Inventory.RemainWeight;

        foreach (ItemObject itemObj in GameManager.Instance.GetNearestRObjectsFromType<ItemObject>(pawn.MapTilePosition))
        {
            List<(Vector2Int, float)> tmpPath = new List<(Vector2Int, float)>();
            if (GameManager.Instance.GetAITagSystem(AITagSubject.PickupItem).IsTagged(itemObj) == false
                && GameManager.Instance.FindPath(pawn.MapTilePosition, itemObj.MapTilePosition, ref tmpPath) == true)
            {
                if (nearestItemList.TryGetValue(itemObj.Desc, out List<ItemObject> itemList) == false)
                    nearestItemList.Add(itemObj.Desc, itemList = new List<ItemObject>());

                itemList.Add(itemObj);
            }
        }

        PlanObject firstTarget = null;
        foreach (PlanObject planObj in GameManager.Instance.GetNearestRObjectsFromType<PlanObject>(pawn.MapTilePosition))
        {
            List<(Vector2Int, float)> tmpPath = new List<(Vector2Int, float)>();
            var remainItemList = planObj.GetRemainReqItemList();
            if (GameManager.Instance.GetAITagSystem(AITagSubject.Build).IsTagged(planObj) == false 
                && (firstTarget == null || VectorExt.Get8DirectionLength(pawn.MapTilePosition, planObj.MapTilePosition) <= 5)
                && remainItemList.Count > 0
                && GameManager.Instance.FindPath(pawn.MapTilePosition, planObj.MapTilePosition, ref tmpPath) == true)
            {

            }
        }

        yield return State.Complete;
    }
}

public class PickupItem : Move
{
    
}

//이동
public abstract class Move : ActionTask
{
    protected abstract bool TryFindPath(Pawn pawn, ref List<(Vector2Int, float)> path);

    public override IEnumerable<State> Run(Pawn pawn)
    {
        List<(Vector2Int, float)> path = new List<(Vector2Int, float)>();

        if (TryFindPath(pawn, ref path) == false)
        {
            yield return State.Failed;
        }

        float remainTimeDelta = Time.deltaTime;

        for (int pathIndex = path.Count > 1 ? 1 : 0; pathIndex < path.Count; ++pathIndex)
        {
            var nextPosWeight = path[pathIndex];

            Vector2 nextDestination = nextPosWeight.Item1 + new Vector2(0.5f, 0.5f);
            Vector2 movingDir = ((nextPosWeight.Item1 - pawn.MapTilePosition)).ToVec2Float().normalized;
            Vector2 toNextDestDir = (nextDestination - pawn.MapPosition).normalized;

            if (Mathf.Approximately(toNextDestDir.x, movingDir.x) == false
                || Mathf.Approximately(toNextDestDir.y, movingDir.y) == false)
            {
                Vector2 currentTilePos = pawn.MapTilePosition + new Vector2(0.5f, 0.5f);
                if (Vector2.Distance(nextDestination, pawn.MapPosition) > Vector2.Distance(nextDestination, currentTilePos))
                {
                    nextDestination = currentTilePos;
                }
            }

            while (pawn.MapPosition != nextDestination)
            {
                float remainDistance = Vector2.Distance(pawn.MapPosition, nextDestination);

                float speed = pawn.Speed / nextPosWeight.Item2;

                float moveDelta = speed * remainTimeDelta;

                remainDistance = remainDistance - moveDelta;

                if (remainDistance > 0)
                {
                    pawn.MapPosition += moveDelta * (nextDestination - pawn.MapPosition).normalized;
                    yield return State.Running;
                    remainTimeDelta = Time.deltaTime;
                }
                else
                {
                    pawn.MapPosition = nextDestination;
                    remainTimeDelta = (-remainDistance) / speed;
                }
            }
        }

        yield return State.Complete;
    }
}

public class MovePosition : Move
{
    private Vector2Int goalPosition;

    public MovePosition(Vector2Int goalPosition)
    {
        this.goalPosition = goalPosition;
    }

    protected override bool TryFindPath(Pawn pawn, ref List<(Vector2Int, float)> path)
    {
        return GameManager.Instance.FindPath(pawn.MapTilePosition, goalPosition, ref path);
    }
}

public class DirectControl : CompositeTask
{
    public int ChildCount { get => childList.Count; }

    public override IEnumerable<State> Run(Pawn pawn)
    {
        while (childList.Count > 0)
        {
            foreach (State state in childList[0].Run(pawn))
            {
                if (state == State.Running)
                    yield return State.Running;
                else
                    break;
            }
            childList.RemoveAt(0);
        }

        yield return State.Complete;
    }

    public void Clear()
    {
        childList.Clear();
    }
}

public class PawnAI
{
    private Pawn pawn;
    private Selector aiSelector;
    private DirectControl directControl;
    private IEnumerator<State> runningState;

    private bool isDirectControl;

    public bool IsDirectControl
    {
        get => isDirectControl;
        private set
        {
            if (isDirectControl == value)
                return;

            runningState = null;
            isDirectControl = value;
        }
    }

    public PawnAI(Pawn pawn)
    {
        this.pawn = pawn;
        aiSelector = new Selector();
        directControl = new DirectControl();
    }

    public void UpdateTick()
    {
        if (runningState?.Current == State.Running)
            runningState.MoveNext();
        else
        {
            if (directControl.ChildCount > 0)
                runningState = directControl.Run(pawn).GetEnumerator();
            else if (isDirectControl == false)
                runningState = aiSelector.Run(pawn).GetEnumerator();

            runningState?.MoveNext();
        }
    }

    public void Reset()
    {
        foreach (AITagSubject tag in Enum.GetValues(typeof(AITagSubject)))
            GameManager.Instance.GetAITagSystem(tag).UntagAllTagOfPawn(pawn);

        directControl.Clear();
        runningState = null;
    }
    public void AddDirectOrder(Node node)
    {
        directControl.AddChild(node);
    }
}
