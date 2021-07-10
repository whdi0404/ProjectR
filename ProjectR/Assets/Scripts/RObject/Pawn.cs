using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : RObject
{
	public PawnAI PawnAI { get; private set; }

	public Pawn(DataContainer properties) : base(properties)
	{
		PawnAI = new PawnAI();
		this.VisualImage = Resources.Load<Sprite>("PawnTextures/pawn");
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		PawnAI.UpdateTick(this);
	}

	public override void VisualUpdate(float dt)
	{
	}
}