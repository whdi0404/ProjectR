using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
using System.Linq;
using System;
using System.Diagnostics;

public class Pickup : ActionTask<ItemReserver>
{
    public Pickup(ItemReserver reserver) : base(reserver)
    {
    }

    public override IEnumerable<State> Run()
    {
        if (reserver.Dest.ParentObject != ParentAI.Pawn)
        {
            yield return State.Failed;
        }

        //Move
        if (Pawn.SetMove(reserver.Source.ParentObject.MapTilePosition) == false)
        {
            yield return State.Failed;
        }
        while (Pawn.IsMoving == true)
        {
            yield return State.Running;
        }

        reserver.Source.MoveToOtherContainer(reserver.Dest, reserver.Item, out Item moveFailed);

        yield return State.Complete;
    }
}

public class Haul : ActionTask<ItemReserver>
{
    public Haul(ItemReserver reserver) : base(reserver)
    {
    }

    public override IEnumerable<State> Run()
    {
        if (reserver.Source.ParentObject != Pawn)
        {
            yield return State.Failed;
        }

        //Move
        if (Pawn.SetMove(reserver.Dest.ParentObject.MapTilePosition) == false)
        {
            yield return State.Failed;
        }
        while (Pawn.IsMoving == true)
        {
            yield return State.Running;
        }

        reserver.Source.MoveToOtherContainer(reserver.Dest, reserver.Item, out Item moveFailed);

        yield return State.Complete;
    }
}

public class Work : ActionTask<WorkReserver>
{
    public Work(WorkReserver reserver) : base(reserver)
    {
    }

    public override IEnumerable<State> Run()
    {
        if (reserver.Source.AI.Pawn != Pawn || reserver.Dest.IsWorkable == false)
        {
            yield return State.Failed;
        }

        //Move
        if (Pawn.SetMove(reserver.Dest.WorkPlace.MapTilePosition) == false)
        {
            yield return State.Failed;
        }
        while (Pawn.IsMoving == true)
        {
            yield return State.Running;
        }

        if (reserver.Dest.IsWorkable == false)
            yield return State.Failed;//Complete,Fail이 의미가 있나?

        while (reserver.Dest.Work(Pawn) == false)
            yield return State.Running;

        yield return State.Complete;
    }
}

public class PawnAI
{
    public Pawn Pawn { get; private set; }
    private AIRoot aiRoot;
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
        this.Pawn = pawn;
        aiRoot = new AIRoot(this);
        runningState = aiRoot.Run().GetEnumerator();
    }

    public void UpdateTick()
    {
        runningState.MoveNext();
    }

    public void Reset()
    {
        runningState = null;
    }

    public void AddAINode(AINode node)
    {
        aiRoot.AddChild(node);
    }

    public void CancelAll()
    {
    }
}
