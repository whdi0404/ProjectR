using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
using System.Linq;
using System;
using System.Diagnostics;
//밥먹기

//잠자기

//건설하기
public class TagBuild : ActionTask
{
    public override IEnumerable<State> Run(Pawn pawn)
    {
        var taggedList = GameManager.Instance.GetAITagSystem(AITagSubject.Build).GetTaggedObjectsOfPawn(pawn);
        if (taggedList != null && taggedList.Count > 0)
            yield return State.Complete;

        foreach (PlanObject planObj in GameManager.Instance.ObjectManager.GetNearestObjectFromIndexId<PlanObject>("Plan/Build", pawn.MapTilePosition))
        {
            if (GameManager.Instance.GetAITagSystem(AITagSubject.Build).IsTagged(planObj) == false
                && planObj.GetRemainReqItemList().Count == 0
                && GameManager.Instance.IsReachable(pawn.MapTilePosition, planObj.MapTilePosition) == true)
            {
                GameManager.Instance.GetAITagSystem(AITagSubject.Build).Tag(pawn, planObj);
                yield return State.Complete;
            }
        }

        yield return State.Failed;
    }
}

public class Build : Move
{
    private PlanObject planObject;

    protected override bool TryFindPath(Pawn pawn, ref List<(Vector2Int, float)> path)
    {
        var taggedList = GameManager.Instance.GetAITagSystem(AITagSubject.Build).GetTaggedObjectsOfPawn(pawn);
        if (taggedList == null || taggedList.Count == 0)
            return false;

        planObject = taggedList[0].Item1 as PlanObject;
        foreach (var position in planObject.GetNearestAdjecentTile(pawn.MapTilePosition, false))
        {
            if (GameManager.Instance.FindPath(pawn.MapTilePosition, position, ref path) == true)
                return true;
        }

        return false;
    }

    public override IEnumerable<State> Run(Pawn pawn)
    {
        foreach (var state in base.Run(pawn))
        {
            if (state == State.Complete)
                break;
            yield return state;
        }

        var taggedList = GameManager.Instance.GetAITagSystem(AITagSubject.Build).GetTaggedObjectsOfPawn(pawn);
        if (taggedList == null || taggedList.Count == 0)
            yield return State.Failed;

        planObject = taggedList[0].Item1 as PlanObject;

        if (planObject?.IsAdjeceny(pawn) == false)
            yield return State.Failed;

        while (planObject.RemainWorkload > 0)
        {
            planObject.Work(Time.deltaTime * 5.0f);
            yield return State.Running;
        }

        yield return State.Complete;
    }
}


//만들기

//재료 옮기기
public class TagBuildTransport : ActionTask
{
    public override IEnumerable<State> Run(Pawn pawn)
    {
        //var taggedList = GameManager.Instance.GetAITagSystem(AITagSubject.Build).GetTaggedObjectsOfPawn(pawn);
        //if (taggedList != null && taggedList.Count > 0)
        //    yield return State.Complete;

        SmartDictionary<ItemDataDescriptor, List<ItemObject>> nearestItemList = new SmartDictionary<ItemDataDescriptor, List<ItemObject>>();
        float remainWeight = pawn.Inventory.RemainWeight;

        foreach (ItemObject itemObj in GameManager.Instance.ObjectManager.GetNearestObjectFromIndexId<ItemObject>("Item", pawn.MapTilePosition))
        {
            if (GameManager.Instance.GetAITagSystem(AITagSubject.PickupItem).IsTagged(itemObj) == false)
            {
                if (nearestItemList.TryGetValue(itemObj.Desc, out List<ItemObject> itemList) == false)
                    nearestItemList.Add(itemObj.Desc, itemList = new List<ItemObject>());

                itemList.Add(itemObj);
            }
        }
        //{PlanObj 하나 Tag하고, 아이템 Tag, 줍기} while로 반복?
        foreach (PlanObject planObj in GameManager.Instance.ObjectManager.GetNearestObjectFromIndexId<PlanObject>("Plan/Build", pawn.MapTilePosition))
        {
            var remainItemList = planObj.GetRemainReqItemList();
            if ( GameManager.Instance.GetAITagSystem( AITagSubject.Build ).IsTagged( planObj ) == false
                //첫번째로 Tag된 planobj를 기준으로 다시  설정할까
                //&& (firstTarget == null || VectorExt.Get8DirectionLength(pawn.MapTilePosition, planObj.MapTilePosition) <= 5)
                && remainItemList.Count > 0
                && GameManager.Instance.IsReachable( pawn.MapTilePosition, planObj.MapTilePosition ) == true )
            {
                //float reqWeight = remainItemList.Sum( reqItem => reqItem.ItemDesc.Weight * reqItem.Amount );
                bool tagItem = false;
                foreach ( var reqItem in remainItemList )
                {
                    if ( nearestItemList[ reqItem.ItemDesc ]?.Sum( itemObj => itemObj.Amount ) >= reqItem.Amount )
                    {
                        if ( remainWeight < reqItem.ItemDesc.Weight )
                            continue;

                        int remainReqItemAmount = reqItem.Amount;
                        foreach ( var itemObj in nearestItemList[ reqItem.ItemDesc ] )
                        {
                            int grabAmount = Mathf.Min( remainReqItemAmount, itemObj.Amount );

                            remainReqItemAmount -= grabAmount;
                            remainWeight -= grabAmount * reqItem.ItemDesc.Weight;
                            GameManager.Instance.GetAITagSystem( AITagSubject.PickupItem ).Tag( pawn, itemObj, grabAmount );

                            if ( remainWeight <= 0 || remainReqItemAmount <= 0 )
                                break;
                        }

                        tagItem = true;
                    }
                }

                if ( tagItem == true )
                {
                    GameManager.Instance.GetAITagSystem( AITagSubject.Build ).Tag( pawn, planObj );
                    yield return State.Complete;
                    yield break;
                }
            }
        }

        yield return State.Failed;
    }
}

public class PickupItem : Move
{
	(ItemObject, int) pickupItem;
	protected override bool TryFindPath( Pawn pawn, ref List<(Vector2Int, float)> path )
	{
        var taggedItemList = GameManager.Instance.GetAITagSystem( AITagSubject.PickupItem ).GetTaggedObjectsOfPawn( pawn );
        if ( taggedItemList == null || taggedItemList.Count == 0 ||taggedItemList[0].Item1 is ItemObject == false )
            return false;

		pickupItem = (taggedItemList[ 0 ].Item1 as ItemObject, ( int )taggedItemList[ 0 ].Item2);
		return GameManager.Instance.FindPath( pawn.MapTilePosition, pickupItem.Item1.MapTilePosition, ref path );
	}

	public override IEnumerable<State> Run( Pawn pawn )
	{
        foreach ( var state in base.Run( pawn ) )
        {
            if (state == State.Complete)
                break;
            yield return state;
        }

        GameManager.Instance.GetAITagSystem( AITagSubject.PickupItem ).UnTag( pawn, pickupItem.Item1 );
        pawn.Inventory.AddItems( pickupItem.Item1, pickupItem.Item2 );

        yield return State.Complete;
    }
}

public class HaulPlanObject : Move
{
    PlanObject haulTarget = null;
	protected override bool TryFindPath( Pawn pawn, ref List<(Vector2Int, float)> path )
	{
        var taggedItemList = GameManager.Instance.GetAITagSystem( AITagSubject.Build ).GetTaggedObjectsOfPawn( pawn );
        if ( taggedItemList == null || taggedItemList.Count == 0 || taggedItemList[ 0 ].Item1 is PlanObject == false )
            return false;

        haulTarget = taggedItemList[ 0 ].Item1 as PlanObject;
        
        //IF노드로 뺼까
        var remainReqItemList = haulTarget.GetRemainReqItemList();
        if ( remainReqItemList.Exists( reqItem => pawn.Inventory.GetItemAmount( reqItem.ItemDesc ) > 0 ) == false )
            return false;

        return GameManager.Instance.FindPath( pawn.MapTilePosition, haulTarget.MapTilePosition, ref path );
    }

    public override IEnumerable<State> Run( Pawn pawn )
    {
        foreach ( var state in base.Run( pawn ) )
        {
            if (state == State.Complete)
                break;
            yield return state;
        }

        GameManager.Instance.GetAITagSystem( AITagSubject.Build ).UnTag( pawn, haulTarget );
        foreach ( var remainItem in haulTarget.GetRemainReqItemList() )
        {
			int haulAmount = Mathf.Min( remainItem.Amount, pawn.Inventory.GetItemAmount( remainItem.ItemDesc ) );
			pawn.Inventory.MoveToOtherInventory( haulTarget.Inventory, remainItem.ItemDesc, haulAmount );
		}

		yield return State.Complete;
    }
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
        
        //건설 운반
        Selector buildHaulSelector = new Selector();
        {
            Sequence pickupSequence = new Sequence();
            {
                pickupSequence.AddChild( new TagBuildTransport() );
                pickupSequence.AddChild( new PickupItem() );
            }
            buildHaulSelector.AddChild( pickupSequence );

            buildHaulSelector.AddChild( new HaulPlanObject() );
        }
        aiSelector.AddChild(buildHaulSelector);

        //건설
        Sequence buildSequence = new Sequence();
        {
            buildSequence.AddChild(new TagBuild());
            buildSequence.AddChild(new Build());
        }
        aiSelector.AddChild(buildSequence);
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
