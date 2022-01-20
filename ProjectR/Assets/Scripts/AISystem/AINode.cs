using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
	Failed,
	Running,
	Complete,
};

public abstract class AINode
{
	public event Action<AINode> onCancel;
	public event Action<AINode> onComplete;
	public PawnAI ParentAI { get; protected set; }
	public Pawn Pawn { get => ParentAI?.Pawn; }

	public bool IsEnd { get; private set; } = false;

	public void SetParent(PawnAI pawnAI)
	{
		ParentAI = pawnAI;
	}

	public abstract IEnumerable<AIState> Run();

	public virtual void Cancel()
	{
		if (IsEnd) return;
		IsEnd = true;
		onCancel?.Invoke(this);
	}
	public virtual void Complete()
	{
		if (IsEnd) return;
		IsEnd = true;
		onComplete?.Invoke(this);
	}

}

public abstract class ActionTask<TReserver> : AINode where TReserver : ReserverBase
{
	protected TReserver reserver;
	public ActionTask(TReserver reserver)
	{
		this.reserver = reserver;
		reserver.onDestroyAfterComplete += Complete;
		reserver.onDestroyBeforeComplete += Cancel;
	}

	public override void Complete()
	{
		reserver.onDestroyAfterComplete -= Complete;
		reserver.onDestroyBeforeComplete -= Cancel;
		reserver.Complete();
		reserver.Destroy();
		base.Complete();
	}

	public override void Cancel()
	{
		reserver.onDestroyAfterComplete -= Complete;
		reserver.onDestroyBeforeComplete -= Cancel;
		reserver.Destroy();
		base.Cancel();
	}
}

public abstract class CompositeTask : AINode
{
	protected List<AINode> children = new List<AINode>();
	protected List<AINode> GetChildren() { return children; }

	public void AddChild(AINode node)
	{
		node.SetParent(ParentAI);
		children.Add(node);
		node.onComplete += OnCompleteChild;
		node.onCancel += OnCancelChild;
	}

	protected virtual void OnCompleteChild(AINode child)
	{
		children.Remove(child);
	}

	protected virtual void OnCancelChild(AINode child)
	{
		children.Remove(child);
	}
}

//public abstract class AIEvaluator
//{
//    public abstract bool Evaluate(PawnAI pawnAI, out Node node);
//}

public class AIRoot : CompositeTask
{
	public AIRoot(PawnAI pawnAI)
	{
		ParentAI = pawnAI;
	}

	public override IEnumerable<AIState> Run()
	{
		while (true)
		{
			if (children.Count == 0)
			{
				yield return AIState.Running;
				EvaluateAI();
			}
			else
			{
				AINode child = children[0];
				foreach (AIState state in child.Run())
				{
					if (children[0] != child)
					{
						break;
					}

					if (state == AIState.Failed)
					{
						child.Cancel();
						break;
					}

					if (state == AIState.Complete)
					{
						child.Complete();
						break;
					}

					yield return state;
				}
			}
		}
	}

	//이것들을 어디다가 뺄지 생각해보자.
	public void EvaluateAI()
	{
		if (DoAction() == false)
		{
			if (EvaluateWork("Build") == true)
				return;
			if (EvaluateCarryItem("Build") == true)
				return;
		}
	}

	public bool DoAction()
	{
		var itemReserveSystem = GameManager.Instance.ItemSystem.ReserveSystem;
		var workReserveSystem = GameManager.Instance.WorkSystem.ReserveSystem;

		var pickupList = itemReserveSystem.GetAllReserverFromDest(Pawn.Inventory);
		foreach (var reserver in pickupList)
		{
			AddChild(new Pickup(reserver));
			return true;
		}

		var haulList = itemReserveSystem.GetAllReserverFromSource(Pawn.Inventory);
		foreach (var reserver in haulList)
		{
			AddChild(new Haul(reserver));
			return true;
		}

		var workList = workReserveSystem.GetAllReserverFromSource(Pawn);
		foreach (var reserver in workList)
		{
			AddChild(new Work(reserver));
			return true;
		}

		return false;
	}

	public bool EvaluateCarryItem(string workid)
	{
		var objManager = GameManager.Instance.ObjectManager;
		var itemSystem = GameManager.Instance.ItemSystem;

		bool isSuccess = false;

		if (Pawn.Inventory.RemainWeightIncludeIn == 0)
			return false;

		foreach (var workObj in objManager.GetNearestObjectFromIndexId<WorkPlaceObject>($"Work/{workid}", Pawn.MapTilePosition))
		{
			var work = workObj.GetWork(Pawn);
			if (work == null)
				continue;

			foreach (Item req in work.WorkHolder.GetReserveRemainReqItemList())
			{
				Item reqItem = req;
				if (Pawn.Inventory.EnableToAddIncludeInItemAmount(reqItem.ItemDesc) == 0)
					continue;

				foreach (var itemObj in objManager.GetNearestObjectFromIndexId<ItemObject>($"Item/{reqItem.ItemDesc.Id}", Pawn.MapTilePosition))
				{
					int carriableAmount = Pawn.Inventory.EnableToAddIncludeInItemAmount(reqItem.ItemDesc);
					int itemAmount = itemObj.ItemContainer.ItemExcludeOut.Amount;
					if (itemAmount == 0)
						continue;

					carriableAmount = Mathf.Min(Mathf.Min(carriableAmount, itemAmount), reqItem.Amount);

					isSuccess = true;
					//pickup reserve
					itemSystem.ReserveSystem.AddReserver(new ItemReserver(itemObj.ItemContainer, Pawn.Inventory, new Item(reqItem.ItemDesc, carriableAmount)));

					//haul reserve
					itemSystem.ReserveSystem.AddReserver(new ItemReserver(Pawn.Inventory, work.WorkHolder, new Item(reqItem.ItemDesc, carriableAmount)));

					reqItem.Amount -= carriableAmount;

					if (reqItem.Amount <= 0)
						break;

					if (Pawn.Inventory.EnableToAddIncludeInItemAmount(reqItem.ItemDesc) == 0)
						break;
				}
			}
		}

		return isSuccess;
	}

	public bool EvaluateWork(string workid)
	{
		var objManager = GameManager.Instance.ObjectManager;
		var workSystem = GameManager.Instance.WorkSystem;

		foreach (var workObj in objManager.GetNearestObjectFromIndexId<WorkPlaceObject>($"Work/{workid}", Pawn.MapTilePosition))
		{
			var work = workObj.GetWork(Pawn);
			if (work != null && work.IsWorkable && workSystem.ReserveSystem.GetAllReserverFromDest(work).Count == 0)
			{
				workSystem.ReserveSystem.AddReserver(new WorkReserver(Pawn, work));
				return true;
			}
		}
		return false;
	}
}