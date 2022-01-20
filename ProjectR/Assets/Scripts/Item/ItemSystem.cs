using System.Collections.Generic;
using UnityEngine;

public class ItemReserverSystem : ReserveSystemBase<ItemReserver, ItemContainer, ItemContainer>
{
}

public class ItemReserver : ReserverBase<ItemContainer, ItemContainer>
{
	public Item Item { get; private set; }

	public ItemReserver(ItemContainer source, ItemContainer dest, Item item) : base(source, dest)
	{
		Item = item;
	}

	public override void Destroy()
	{
		GameManager.Instance.ItemSystem.ReserveSystem.RemoveReserver(this);
	}
}

public partial class ItemSystem : IObjectManagerListener
{
	private SmartDictionary<RObject, List<ItemContainer>> containierDict = new SmartDictionary<RObject, List<ItemContainer>>();

	public ItemReserverSystem ReserveSystem { get; private set; }

	public ItemSystem()
	{
		ReserveSystem = new ItemReserverSystem();
	}

	public bool DropItem(Vector2Int pos, Item dropItem, out Item dropFailed)
	{
		dropFailed = dropItem;

		if (dropItem.Amount == 0)
			return true;

		RegionSystem regionSystem = GameManager.Instance.WorldMap.RegionSystem;
		ObjectManager objectManager = GameManager.Instance.ObjectManager;

		if (regionSystem.GetRegionFromTilePos(pos, out LocalRegion posRegion) == false || posRegion.IsClosedRegion == true)
			return false;

		var reachableRegions = regionSystem.GetAllReachableRegions(posRegion);
		Dictionary<Vector2Int, ItemObject> existItems = new Dictionary<Vector2Int, ItemObject>();

		foreach (var region in reachableRegions)
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
			PriorityQueue<NearNode<Vector2Int>> pq = new PriorityQueue<NearNode<Vector2Int>>();

			foreach (var region in reachableRegions)
			{
				foreach (var tilepos in region.GetTiles())
				{
					NearNode<Vector2Int> serachTile = new NearNode<Vector2Int>();
					serachTile.position = tilepos;
					serachTile.distance = VectorExt.Get8DirectionLength(serachTile.position, pos);

					pq.Enqueue(serachTile);
				}
			}

			while (pq.Count != 0)
			{
				var node = pq.Dequeue();
				if (existItems.TryGetValue(node.position, out var itemObj) == true)
				{
					if (itemObj.ItemContainer.Item.ItemDesc == dropItem.ItemDesc)
					{
						itemObj.ItemContainer.AddItems(dropFailed, out dropFailed);
					}
				}
				else
				{
					ItemObject newItem = new ItemObject(dropItem.ItemDesc);
					newItem.ItemContainer.AddItems(dropFailed, out dropFailed);
					newItem.MapTilePosition = node.position;
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
			if (GameManager.Instance.ItemSystem.DropItem(pos, item, out Item dropFailed) == true)
				dropFailedList.Add(dropFailed);
		}

		return dropFailedList.Count == 0;
	}

	private List<ItemContainer> GetOrMakeContainerList(RObject parentObj)
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

	public void OnCreateObject(RObject rObject)
	{
	}

	public void OnDestroyObject(RObject rObject)
	{
		if (containierDict.TryGetValue(rObject, out List<ItemContainer> containerList))
		{
			foreach (var itemContainer in containerList)
			{
				itemContainer.OnDestroy();
			}
			containierDict.Remove(rObject);
		}
	}
}