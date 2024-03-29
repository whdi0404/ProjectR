using System.Collections.Generic;
using Table;
using UnityEngine;

public class StructurePlanningDescriptor : Descriptor
{
	[GoogleRefColumn(typeof(TileAtlasInfoTable))]
	public AtlasInfoDescriptor Structure { get; private set; }

	[GoogleRefColumn(typeof(InstallObjectTable))]
	public InstallObjectDataDescriptor InstallObject { get; private set; }

	[GoogleColumn]
	public string RequireItems { get; private set; }

	[GoogleColumn]
	public float Workload { get; private set; }

	public List<Item> ReqItemList { get; private set; }

	public Vector2Int Size { get => InstallObject?.Size ?? new Vector2Int(1, 1); }

	public string Name
	{
		get
		{
			return ((Descriptor)Structure ?? InstallObject).Id;
		}
	}

	public void OnLoaded()
	{
		string[] splitStrings = RequireItems.Split(',');

		ReqItemList = new List<Item>();
		int arrayAmountHalf = splitStrings.Length / 2;
		for (int i = 0; i < arrayAmountHalf; ++i)
		{
			Item reqItem = new Item();
			reqItem.ItemDesc = TableManager.GetTable<ItemDataTable>().Find(splitStrings[i * 2]);
			reqItem.Amount = int.Parse(splitStrings[i * 2 + 1]);

			ReqItemList.Add(reqItem);
		}
	}
}

[GoogleWorkSheet("StructurePlanningTable", "StructurePlanning")]
public class StructurePlanningTable : Sheet<StructurePlanningDescriptor>
{
	public override void OnLoaded()
	{
		base.OnLoaded();

		foreach (var desc in All())
			desc.OnLoaded();
	}
}