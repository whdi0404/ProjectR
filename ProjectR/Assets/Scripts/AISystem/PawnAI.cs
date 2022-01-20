using System.Collections.Generic;

public class Pickup : ActionTask<ItemReserver>
{
	public Pickup(ItemReserver reserver) : base(reserver)
	{
	}

	public override IEnumerable<AIState> Run()
	{
		if (reserver.Dest.ParentObject != ParentAI.Pawn)
		{
			yield return AIState.Failed;
		}

		//Move
		if (Pawn.SetMove(reserver.Source.ParentObject.MapTilePosition) == false)
		{
			yield return AIState.Failed;
		}
		while (Pawn.IsMoving == true)
		{
			yield return AIState.Running;
		}

		reserver.Source.MoveToOtherContainer(reserver.Dest, reserver.Item, out Item moveFailed);

		yield return AIState.Complete;
	}
}

public class Haul : ActionTask<ItemReserver>
{
	public Haul(ItemReserver reserver) : base(reserver)
	{
	}

	public override IEnumerable<AIState> Run()
	{
		if (reserver.Source.ParentObject != Pawn)
		{
			yield return AIState.Failed;
		}

		//Move
		if (Pawn.SetMove(reserver.Dest.ParentObject.MapTilePosition) == false)
		{
			yield return AIState.Failed;
		}
		while (Pawn.IsMoving == true)
		{
			yield return AIState.Running;
		}

		reserver.Source.MoveToOtherContainer(reserver.Dest, reserver.Item, out Item moveFailed);

		yield return AIState.Complete;
	}
}

public class Work : ActionTask<WorkReserver>
{
	public Work(WorkReserver reserver) : base(reserver)
	{
	}

	public override IEnumerable<AIState> Run()
	{
		if (reserver.Source.AI.Pawn != Pawn || reserver.Dest.IsWorkable == false)
		{
			yield return AIState.Failed;
		}

		//Move
		if (Pawn.SetMove(reserver.Dest.WorkPlace.MapTilePosition) == false)
		{
			yield return AIState.Failed;
		}
		while (Pawn.IsMoving == true)
		{
			yield return AIState.Running;
		}

		if (reserver.Dest.IsWorkable == false)
			yield return AIState.Failed;//Complete,Fail이 의미가 있나?

		while (reserver.Dest.Work(Pawn) == false)
			yield return AIState.Running;

		yield return AIState.Complete;
	}
}

public class PawnAI
{
	public Pawn Pawn { get; private set; }
	private AIRoot aiRoot;
	private IEnumerator<AIState> runningState;

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
		runningState.Reset();// = null;
	}

	public void AddAINode(AINode node)
	{
		aiRoot.AddChild(node);
	}

	public void CancelAll()
	{
	}
}