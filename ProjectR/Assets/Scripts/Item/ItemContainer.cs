using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class ItemContainer
{
	public RObject ParentObject { get; protected set; }

	public SmartDictionary<ItemDataDescriptor, int> ItemDict { get; protected set; } = new SmartDictionary<ItemDataDescriptor, int>();

	protected ItemContainer(RObject parentObject)
	{
		ParentObject = parentObject;
	}

	public abstract bool AddItems(Item addItem, out Item addFailed);

	public virtual bool DropItems(Vector2Int pos, Item dropItem, out Item dropFailed)
	{
		dropItem.Amount = Mathf.Min(dropItem.Amount, ItemDict[dropItem.ItemDesc]);

		bool retVal = GameManager.Instance.ItemSystem.DropItem(pos, dropItem, out dropFailed);
		dropItem.Amount -= dropFailed.Amount;
		RemoveItems(dropItem, out Item removeFailed);

		return retVal;
	}

	public virtual bool RemoveItems(Item removeItem, out Item removeFailed)
	{
		removeFailed = removeItem;
		removeFailed.Amount = Mathf.Max(removeItem.Amount - ItemDict[removeItem.ItemDesc], 0);
		ItemDict[removeItem.ItemDesc] -= removeItem.Amount - removeFailed.Amount;

		return removeFailed.Amount <= 0;
	}

	public void RemoveAllItems()
	{
		ItemDict.Clear();
	}

	public bool MoveToOtherContainer(ItemContainer other, Item moveItem, out Item moveFailed)
	{
		moveItem.Amount = Mathf.Min(moveItem.Amount, ItemDict[moveItem.ItemDesc]);

		bool retVal = other.AddItems(moveItem, out moveFailed);
		moveItem.Amount -= moveFailed.Amount;
		RemoveItems(moveItem, out Item removeFailed);

		return retVal;
	}

	public virtual void OnDestroy()
	{
		foreach (var kv in ItemDict)
		{
			Item item = new Item(kv.Key, kv.Value);
			GameManager.Instance.ItemSystem.DropItem(ParentObject.MapTilePosition, item, out Item dropFailed);
		}
		ItemDict.Clear();

		GameManager.Instance.ItemSystem.ReserveSystem.RemoveAllReserverFromSource(this);
		GameManager.Instance.ItemSystem.ReserveSystem.RemoveAllReserverFromDest(this);
	}

	public SmartDictionary<ItemDataDescriptor, int> GetItemList(bool excludeOut, bool includeIn)
	{
		SmartDictionary<ItemDataDescriptor, int> retVal = new SmartDictionary<ItemDataDescriptor, int>(ItemDict);

		ItemReserverSystem reserveSystem = GameManager.Instance.ItemSystem.ReserveSystem;

		if (excludeOut == true)
		{
			List<ItemReserver> itemReserverList = reserveSystem.GetAllReserverFromSource(this);
			foreach (ItemReserver itemReserver in itemReserverList)
			{
				var item = itemReserver.Item;
				retVal[item.ItemDesc] -= item.Amount;
			}
		}

		if (includeIn == true)
		{
			List<ItemReserver> itemReserverList = reserveSystem.GetAllReserverFromDest(this);
			foreach (ItemReserver itemReserver in itemReserverList)
			{
				var item = itemReserver.Item;
				retVal[item.ItemDesc] += item.Amount;
			}
		}

		return retVal;
	}
}

public class SingleItemContainer : ItemContainer
{
	public Item Item
	{
		get
		{
			var v = ItemDict.FirstOrDefault();
			Item item = new Item() { ItemDesc = v.Key, Amount = v.Value };

			return item;
		}
	}

	public Item ItemExcludeOut
	{
		get
		{
			var v = GetItemList(true, false).FirstOrDefault();
			Item item = new Item() { ItemDesc = v.Key, Amount = v.Value };

			return item;
		}
	}

	public SingleItemContainer(RObject parentObject, ItemDataDescriptor desc) : base(parentObject)
	{
		ItemDict.Add(desc, 0);
	}

	public override bool AddItems(Item addItem, out Item addFailed)
	{
		addFailed = addItem;
		if (addItem.ItemDesc != Item.ItemDesc)
		{
			return false;
		}

		addFailed.Amount = Mathf.Max(0, addItem.Amount + Item.Amount - addItem.ItemDesc.StackAmount);
		if (addFailed.Amount > 0)
		{
			ItemDict[addItem.ItemDesc] = addItem.ItemDesc.StackAmount;
			return false;
		}
		else
		{
			ItemDict[addItem.ItemDesc] += addItem.Amount;
			return true;
		}
	}

	public override bool DropItems(Vector2Int pos, Item dropItem, out Item dropFailed)
	{
		throw new System.Exception("SingleItemContainer는 드랍 불가능");
	}

	public override bool RemoveItems(Item removeItem, out Item removeFailed)
	{
		bool retVal = base.RemoveItems(removeItem, out removeFailed);

		if (Item.Amount <= 0)
			GameManager.Instance.ObjectManager.DestroyObject(ParentObject);

		return retVal;
	}

	public override void OnDestroy()
	{
		ItemDict.Clear();
		GameManager.Instance.ItemSystem.ReserveSystem.RemoveAllReserverFromSource(this);
		GameManager.Instance.ItemSystem.ReserveSystem.RemoveAllReserverFromDest(this);
		Debug.Log($"아이템 삭제됨(Pos:{ParentObject.MapTilePosition}, Id:{Item.ItemDesc}, Amount:{Item.Amount})");
	}
}

public class Inventory : ItemContainer
{
	public float WeightLimit { get; private set; }

	public float Weight
	{
		get
		{
			return Item.GetWeights(ItemDict);
		}
	}

	public float WeightIncludeIn
	{
		get
		{
			return Item.GetWeights(GetItemList(false, true));
		}
	}

	public float RemainWeight { get => WeightLimit == 0 ? float.MaxValue : WeightLimit - Weight; }

	public float RemainWeightIncludeIn { get => WeightLimit == 0 ? float.MaxValue : WeightLimit - WeightIncludeIn; }

	public Inventory(RObject parentObject) : base(parentObject)
	{
	}

	public int EnableToAddItemAmount(ItemDataDescriptor itemDesc)
	{
		return itemDesc.Weight == 0 ? 10000 : Mathf.FloorToInt(RemainWeight / itemDesc.Weight);
	}

	public int EnableToAddIncludeInItemAmount(ItemDataDescriptor itemDesc)
	{
		return itemDesc.Weight == 0 ? 10000 : Mathf.FloorToInt(RemainWeightIncludeIn / itemDesc.Weight);
	}

	public void SetWeightLimit(float weightLimit)
	{
		WeightLimit = weightLimit;
	}

	public override bool AddItems(Item addItem, out Item failedItem)
	{
		failedItem = addItem;

		int addableAmount = EnableToAddItemAmount(addItem.ItemDesc);
		int addAmount = Mathf.Min(addableAmount, addItem.Amount);
		failedItem.Amount = Mathf.Max(0, addItem.Amount - addableAmount);

		ItemDict[addItem.ItemDesc] += addAmount;
		return failedItem.Amount <= 0;
	}
}

public class WorkHolder : ItemContainer
{
	public List<Item> RequireItemList { get; private set; }

	public WorkHolder(RObject parentObject, List<Item> requireItemList) : base(parentObject)
	{
		RequireItemList = new List<Item>(requireItemList);
	}

	public IEnumerable<Item> GetRemainReqItemList()
	{
		foreach (var reqItem in RequireItemList)
		{
			int remainAmount = reqItem.Amount - ItemDict[reqItem.ItemDesc];
			if (remainAmount > 0)
				yield return new Item() { ItemDesc = reqItem.ItemDesc, Amount = remainAmount };
		}
	}

	public IEnumerable<Item> GetReserveRemainReqItemList()
	{
		var reserveItemDict = GetItemList(true, true);

		foreach (var reqItem in RequireItemList)
		{
			int remainAmount = reqItem.Amount - reserveItemDict[reqItem.ItemDesc];
			if (remainAmount > 0)
				yield return new Item() { ItemDesc = reqItem.ItemDesc, Amount = remainAmount };
		}
	}

	public override bool AddItems(Item addItem, out Item failedItem)
	{
		failedItem = addItem;

		int addableAmount = RequireItemList.Find(s => s.ItemDesc == addItem.ItemDesc).Amount - ItemDict[addItem.ItemDesc];
		int addAmount = Mathf.Min(addableAmount, addItem.Amount);
		failedItem.Amount = Mathf.Max(0, addItem.Amount - addableAmount);

		ItemDict[addItem.ItemDesc] += addAmount;
		return failedItem.Amount <= 0;
	}
}

public struct Item
{
	public ItemDataDescriptor ItemDesc;
	public int Amount;

	public Item(ItemDataDescriptor itemDesc, int amount)
	{
		ItemDesc = itemDesc;
		Amount = amount;
	}

	public static float GetWeights(Item item)
	{
		return item.ItemDesc.Weight * item.Amount;
	}

	public static float GetWeights(IEnumerable<Item> items)
	{
		float weight = 0;
		foreach (var item in items)
			weight += GetWeights(item);

		return weight;
	}

	public static float GetWeights(IDictionary<ItemDataDescriptor, int> items)
	{
		return items.Sum(s => s.Key.Weight * s.Value);
	}
}