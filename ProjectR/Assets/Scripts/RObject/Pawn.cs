using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
	//(ItemDesc,Amount)
	public SmartDictionary<ItemDataDescriptor, int> ItemDict { get; private set; } = new SmartDictionary<ItemDataDescriptor, int>();

	public float WeightLimit { get; private set; }

	public float Weight
	{
		get 
		{
			return ItemDict.Sum(s => s.Key.Weight * s.Value);
		}
	}

	public float RemainWeight { get => WeightLimit == 0 ? float.MaxValue : WeightLimit - Weight; }

	public bool AddItems(ItemDataDescriptor itemDesc, int amount)
	{
		if (itemDesc.Weight * amount > RemainWeight)
			return false;
		ItemDict[itemDesc] += amount;
		return true;
	}

	public bool AddItems(ItemObject itemObj, int amount)
	{
		if (itemObj.Amount < amount)
			return false;

		if (EnableToAddItemAmount(itemObj.Desc) < amount)
			return false;

		if (AddItems(itemObj.Desc, amount) == false)
			return false;

		itemObj.Amount -= amount;

		if (itemObj.Amount == 0)
			GameManager.Instance.DestroyRObject(itemObj);

		return true;
	}

	public bool AddItems(ItemObject itemObj)
	{
		int addAmount = Mathf.Min(itemObj.Amount, EnableToAddItemAmount(itemObj.Desc));

		return AddItems(itemObj, addAmount);
	}

	public bool DropItems(Vector2Int pos, ItemDataDescriptor itemDesc, int amount)
	{
		if (ItemDict[itemDesc] < amount)
			return false;

		ItemDict[ itemDesc ] -= amount;
		GameManager.Instance.CreateItem(pos, itemDesc, amount);
		return true;
	}

	public bool MoveToOtherInventory(Inventory inventory, ItemDataDescriptor itemDesc, int amount)
	{
		if ( ItemDict[ itemDesc ] < amount )
			return false;

		if ( inventory.RemainWeight < itemDesc.Weight * amount )
			return false;

		ItemDict[ itemDesc ] -= amount;
		inventory.AddItems(itemDesc, amount);
		return true;
	}

	public int GetItemAmount(ItemDataDescriptor itemDesc)
	{
		return ItemDict[itemDesc]; 
	}

    public int EnableToAddItemAmount(ItemDataDescriptor itemDesc)
    {
        return itemDesc.Weight == 0 ? 10000 : Mathf.FloorToInt(RemainWeight / itemDesc.Weight);
    }

	public void SetWeightLimit(float weightLimit)
	{
		WeightLimit = weightLimit;
	}
}

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