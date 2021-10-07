using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pawn : RObject
{
	public PawnAI AI { get; private set; }

	public Inventory Inventory { get; private set; }

	public float Speed { get { return 5.0f; } }

	public Pawn()
	{
		AI = new PawnAI(this);
		Inventory = new Inventory();
		Inventory.SetWeightLimit( 10.0f );
		VisualImage = Resources.Load<Sprite>("PawnTextures/pawn");
		IndexId = $"Pawn";//적,아군,동맹,동물 등 표시
	}

	public override void Update(float dt)
	{
		base.Update(dt);
		AI.UpdateTick();
	}

	public override void VisualUpdate(float dt)
	{

	}

    public override void Destroy()
    {
        base.Destroy();

		foreach (AITagSubject tag in Enum.GetValues(typeof(AITagSubject)))
			GameManager.Instance.GetAITagSystem(tag).UntagAllTagOfPawn(this);
	}
}