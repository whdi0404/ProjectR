using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BT
{
    public enum State 
    { 
        Failed,
        Running,
        Complete,
    };
    
    public abstract class Node
    {
        public event Action<Node> onCancel;
        public event Action<Node> onComplete;
        public PawnAI ParentAI { get; protected set; }
        public Pawn Pawn { get => ParentAI?.Pawn; }

        public bool IsEnd { get; private set; } = false;

        public void SetParent(PawnAI pawnAI)
        {
            ParentAI = pawnAI;
        }
     
        public abstract IEnumerable<State> Run();

        public virtual void Cancel()
        {
            if (IsEnd) return;
            IsEnd = true;
            onCancel?.Invoke(this);
            BreakReserver();
        }
        public virtual void Complete()
        {
            if (IsEnd) return;
            onComplete?.Invoke(this);
            BreakReserver();
            IsEnd = true;
        }

        public void BreakReserver()
        {
            GameManager.Instance.AIReserveSystem.BreakAllReserverFromNode(this);
        }

        public static void DependEachOther(Node a, Node b)
        {
            a.onCancel += (node) => { b.Cancel(); };
            b.onCancel += (node) => { a.Cancel(); };
        }

        public static void Depend(Node child, Node parent)
        {
            parent.onCancel += (node) => { child.Cancel(); };
        }
    }
    public abstract class ActionTask : Node
    {
    }

    public abstract class CompositeTask : Node
    {
        protected List<Node> children = new List<Node>();
        protected List<Node> GetChildren() { return children; }

        public void AddChild(Node node)
        {
            node.SetParent(ParentAI);
            children.Add(node);
            node.onComplete += OnCompleteChild;
            node.onCancel += OnCancelChild;
        }

        protected virtual void OnCompleteChild(Node child)
        {
            children.Remove(child);
        }

        protected virtual void OnCancelChild(Node child)
        {
            children.Remove(child);
        }
    }

    public class AIRoot : CompositeTask
    {
        private bool cancelRunningState;

        public AIRoot(PawnAI pawnAI)
        {
            ParentAI = pawnAI;
        }

        public override IEnumerable<State> Run()
        {
            while (true)
            {
                if (children.Count == 0)
                {
                    EvaluateAI();
                    yield return State.Running;
                }
                else
                {
                    Node child = children[0];
                    foreach (State state in child.Run())
                    {
                        if (cancelRunningState == true)
                        {
                            break;
                        }

                        if (state == State.Failed)
                        {
                            child.Cancel();
                            break;
                        }

                        if (state == State.Complete)
                        {
                            child.Complete();
                            break;
                        }

                        yield return state;
                    }

                    cancelRunningState = false;
                }
            }
        }

        public void EvaluateAI()
        {
            List<Node> nodes;
            if (EvaluateBuildCarry(out nodes) == true
             || false)
            {
                foreach (var node in nodes)
                    AddChild(node);
            }
        }

        public bool EvaluateBuildCarry(out List<Node> result)
        {
            result = null;
            var objManager = GameManager.Instance.ObjectManager;

            if (Pawn.Inventory.RemainReserveWeight == 0)
                return false;

            List<Node> pickups = new List<Node>();
            List<Node> hauls = new List<Node>();

            foreach (var planObj in objManager.GetNearestObjectFromIndexId<BuildPlanObject>("Work/Build", Pawn.MapTilePosition))
            {
                var work = planObj.GetWork(Pawn);
                var reqItemList = work.WorkHolder.GetReserveRemainReqItemList();

                foreach (Item reqItem in reqItemList)
                {
                    Item rReqItem = reqItem;

                    if (Pawn.Inventory.EnableToAddReserveItemAmount(rReqItem.ItemDesc) == 0)
                        continue;

                    foreach (var itemObj in objManager.GetNearestObjectFromIndexId<ItemObject>($"Item/{rReqItem.ItemDesc.Id}", Pawn.MapTilePosition))
                    {
                        int carriableAmount = Pawn.Inventory.EnableToAddReserveItemAmount(rReqItem.ItemDesc);
                        carriableAmount = Mathf.Min(Mathf.Min(carriableAmount, itemObj.ItemContainer.ReserveItem.Amount), rReqItem.Amount);

                        var pickupNode = new Pickup(itemObj.ItemContainer, new List<Item> { new Item(rReqItem.ItemDesc, carriableAmount) });
                        var haulNode = new Haul(work.WorkHolder, new List<Item> { new Item(rReqItem.ItemDesc, carriableAmount) });
                        Node.DependEachOther(pickupNode, haulNode);
                        pickups.Add(pickupNode);
                        hauls.Add(haulNode);

                        rReqItem.Amount -= carriableAmount;

                        if (rReqItem.Amount <= 0)
                            break;

                        if (Pawn.Inventory.EnableToAddReserveItemAmount(rReqItem.ItemDesc) == 0)
                            break;
                    }
                }
            }

            result = new List<Node>();
            result.AddRange(pickups);
            result.AddRange(hauls);

            return result.Count > 0;
        }

        protected override void OnCancelChild(Node node)
        {
            cancelRunningState = children.Count > 0 && node == children[0];
            base.OnCancelChild(node);
        }
        protected override void OnCompleteChild(Node node)
        {
            base.OnCompleteChild(node);
        }

        public override void Cancel()
        {
            base.Cancel();

            foreach (var child in children)
            {
                child.onCancel -= OnCancelChild;
                child.Cancel();
            }

            children.Clear();
        }
    }
}
