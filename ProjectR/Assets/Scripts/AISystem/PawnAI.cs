using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BT;
using System.Linq;
using System;
using System.Diagnostics;

public class Pickup : ActionTask
{
    ItemReserver pickupItemRes;
    public Pickup(ItemContainer itemContainer, List<Item> item)
    {
        pickupItemRes = new ItemReserver(this, itemContainer, ItemReserver.Type.pickup, item);
        GameManager.Instance.AIReserveSystem.RegistReserver(pickupItemRes);
    }

    public override IEnumerable<State> Run()
    {
        if (Pawn.SetMove(pickupItemRes.ItemContainer.ParentObject.MapTilePosition) == false)
        {
            yield return State.Failed;
        }
        while (Pawn.IsMoving == true)
        {
            yield return State.Running;
        }
        foreach (var item in pickupItemRes.ItemList)
        {
            pickupItemRes.ItemContainer.MoveToOtherContainer(ParentAI.Pawn.Inventory, item, out Item moveFailed);
        }

        yield return State.Complete;
    }
}

public class Haul : ActionTask
{
    ItemReserver haulItemRes;
    public Haul(ItemContainer itemContainer, List<Item> item)
    {
        haulItemRes = new ItemReserver(this, itemContainer, ItemReserver.Type.haul, item);
        GameManager.Instance.AIReserveSystem.RegistReserver(haulItemRes);
    }

    public override IEnumerable<State> Run()
    {
        if (ParentAI.Pawn.SetMove(haulItemRes.ItemContainer.ParentObject.MapTilePosition) == false)
        {
            yield return State.Failed;
        }
        while (ParentAI.Pawn.IsMoving == true)
        {
            yield return State.Running;
        }
        foreach (var item in haulItemRes.ItemList)
        {
            ParentAI.Pawn.Inventory.MoveToOtherContainer(haulItemRes.ItemContainer, item, out Item moveFailed);
        }

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

    public void AddAINode(Node node)
    {
        aiRoot.AddChild(node);
    }

    public void CancelAll()
    {
    }
}
