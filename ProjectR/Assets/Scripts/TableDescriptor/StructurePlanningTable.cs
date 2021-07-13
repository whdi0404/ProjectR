using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Table;

public struct RequireItem
{
    public ItemDataDescriptor ItemDesc;
    public int Amount;
}

public class StructurePlanningDescriptor : Descriptor
{
    [GoogleRefColumn(typeof(TileAtlasInfoTable))]
    public AtlasInfoDescriptor Structure { get; private set; }
    [GoogleRefColumn(typeof(InstallObjectTable))]
    public InstallObjectDataDescriptor InstallObject { get; private set; }
    [GoogleColumn]
    public string RequireItems { get; private set; }

    public List<RequireItem> ReqItemList { get; private set; }

    public void OnLoaded()
    {
        string[] splitStrings = RequireItems.Split(',');

        ReqItemList = new List<RequireItem>();
        int arrayAmountHalf = splitStrings.Length / 2;
        for (int i = 0; i < arrayAmountHalf; ++i)
        {
            RequireItem reqItem = new RequireItem();
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