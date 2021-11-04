using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ItemSystem : IObjectManagerListener
{
	private SmartDictionary<RObject, List<ItemContainer>> containierDict = new SmartDictionary<RObject, List<ItemContainer>>();

	public bool DropItem(Vector2Int pos, Item dropItem, out Item dropFailed)
	{
		dropFailed = dropItem;

		if (dropItem.Amount == 0)
			return true;

		RegionSystem regionSystem = GameManager.Instance.WorldMap.RegionSystem;
		ObjectManager objectManager = GameManager.Instance.ObjectManager;

		if (regionSystem.GetRegionFromTilePos(pos, out LocalRegion posRegion) == false || posRegion.IsClosedRegion == true)
			return false;

		HashSet<LocalRegion> reachableRegins = new HashSet<LocalRegion>(regionSystem.GetAllReachableRegions(posRegion));
		Dictionary<Vector2Int, ItemObject> existItems = new Dictionary<Vector2Int, ItemObject>();

		foreach (var region in reachableRegins)
		{
			List<RObject> items = objectManager.GetObjects(region, "Item");
			foreach (var i in items)
				existItems.Add(i.MapTilePosition, (ItemObject)i);
		}

		if (existItems.ContainsKey(pos) == false)
		{
			ItemObject newItem = new ItemObject(dropItem.ItemDesc);
			newItem.ItemContainer.AddItems(dropFailed, out dropFailed);
			newItem.MapTilePosition = pos;
			objectManager.CreateObject(newItem);
		}

		if (dropFailed.Amount > 0)
		{
			int searchAmount = 100;
			//1/3/5/7/9
			//1/2/3/4/5
			for (int i = 2; i <= searchAmount; ++i)
			{
				int sqrtCeil = Mathf.CeilToInt(Mathf.Sqrt(i));
				int squareLength = Mathf.RoundToInt(((sqrtCeil / 2) + 0.5f) * 2);
				int prevSquareLength = squareLength - 2;
				int tt = (squareLength - 1) / 2;

				Vector2Int searchPos;

				int insideIndex = i - prevSquareLength * prevSquareLength;

				int side = Mathf.CeilToInt((float)insideIndex / (squareLength - 1)) - 1;
				int squareIndex = insideIndex - side * (squareLength - 1) - 1;

				if (side == 0)
				{
					Vector2Int startPos = pos + new Vector2Int(tt, -tt + 1);
					searchPos = startPos + new Vector2Int(0, squareIndex);
				}
				else if (side == 1)
				{
					Vector2Int startPos = pos + new Vector2Int(tt - 1, tt);
					searchPos = startPos + new Vector2Int(-squareIndex, 0);
				}
				else if (side == 2)
				{
					Vector2Int startPos = pos + new Vector2Int(-tt, tt - 1);
					searchPos = startPos + new Vector2Int(0, -squareIndex);
				}
				else
				{
					Vector2Int startPos = pos + new Vector2Int(-tt + 1, -tt);
					searchPos = startPos + new Vector2Int(squareIndex, 0);
				}

				if (existItems.TryGetValue(searchPos, out var itemObj) == true)
				{
					if (itemObj.ItemContainer.Item.ItemDesc == dropItem.ItemDesc)
					{
						itemObj.ItemContainer.AddItems(dropFailed, out dropFailed);
					}
				}
				else if (GameManager.Instance.WorldMap.GetTileMovableWeight(searchPos) > 0)
				{
					ItemObject newItem = new ItemObject(dropItem.ItemDesc);
					newItem.ItemContainer.AddItems(dropFailed, out dropFailed);
					newItem.MapTilePosition = pos;
					objectManager.CreateObject(newItem);
				}

				if (dropFailed.Amount <= 0)
					return true;
			}
		}

		return false;
	}

	public bool DropItem(Vector2Int pos, List<Item> dropItems, out List<Item> dropFailedList)
	{
		dropFailedList = new List<Item>();
		foreach (var item in dropItems)
		{
			if (GameManager.Instance.ObjectManager.ItemSystem.DropItem(pos, item, out Item dropFailed) == true)
				dropFailedList.Add(dropFailed);
		}

		return dropFailedList.Count == 0;
	}

	public List<ItemContainer> GetOrMakeContainerList(RObject parentObj)
	{
		if (containierDict.TryGetValue(parentObj, out var containerList) == false)
		{
			containerList = new List<ItemContainer>();
			containierDict.Add(parentObj, containerList);
		}

		return containerList;
	}

	public SingleItemContainer CreateSingleItemContainer(RObject parentObj, ItemDataDescriptor itemDesc)
	{
		SingleItemContainer container = new SingleItemContainer(parentObj, itemDesc);
		GetOrMakeContainerList(parentObj).Add(container);

		return container;
	}
	public Inventory CreateInventory(RObject parentObj)
	{
		Inventory container = new Inventory(parentObj);
		GetOrMakeContainerList(parentObj).Add(container);

		return container;
	}
	public WorkHolder CreateWorkHolder(RObject parentObj, List<Item> requireItem)
	{
		WorkHolder container = new WorkHolder(parentObj, requireItem);
		GetOrMakeContainerList(parentObj).Add(container);

		return container;
	}

	public List<ItemContainer> GetItemContainer(RObject rObj)
	{
		return GetOrMakeContainerList(rObj);
	}

	public void OnCreateObject(RObject rObject)
    {
    }

    public void OnDestroyObject(RObject rObject)
    {
		if (containierDict.TryGetValue(rObject, out List<ItemContainer> containerList))
		{
			foreach (var itemContainer in containerList)
				itemContainer.OnDestroy();
			containierDict.Remove(rObject);
		}
	}

	public void DestroyContainer(ItemContainer itemContainer)
    {
        if (containierDict.TryGetValue(itemContainer.ParentObject, out List<ItemContainer> containerList))
        {
            int index = containerList.IndexOf(itemContainer);
            if (index != -1)
            {
                itemContainer.OnDestroy();
                containerList.RemoveAt(index);
            }
        }
    }
}