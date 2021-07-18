using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : RObject
{
	private PawnAI pawnAI;

	public Pawn(DataContainer properties) : base(properties)
	{
		pawnAI = new PawnAI();
		this.VisualImage = Resources.Load<Sprite>("PawnTextures/pawn");
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		pawnAI.UpdateTick(this);
	}

	public override void VisualUpdate(float dt)
	{
	}

	public void AddDirectOrder(BT.Node node)
	{
		pawnAI.AddDirectOrder(this, node);
	}
}