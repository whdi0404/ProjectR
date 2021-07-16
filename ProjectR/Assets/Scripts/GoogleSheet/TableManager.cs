using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Table;
using UnityEngine;

public static class TableManager
{
    public const string TableRootPath = "Tables";

    private static SmartDictionary<string, Sheet> tables = new SmartDictionary<string, Sheet>();

    public static TSheet GetTable<TSheet>() where TSheet : Sheet
    {
        return GetTable(typeof(TSheet)) as TSheet;
    }
    public static Sheet GetTable(Type type)
    {
        GoogleWorkSheetAttribute workSheetAttr = (type.GetCustomAttributes(false)[0]) as GoogleWorkSheetAttribute;
        if(workSheetAttr != null)
            return GetTable($"{workSheetAttr.SpreadSheetName}.{workSheetAttr.WorkSheetName}");

        CompositeSheetAttribute compositeSheetAttr = (type.GetCustomAttributes(false)[0]) as CompositeSheetAttribute;
        if (compositeSheetAttr != null)
            return GetCompositeTable(type, compositeSheetAttr);

        return null;
    }
    public static Sheet GetTable(string SheetName)
    {
        if (tables.TryGetValue(SheetName, out Sheet sheet) == false || sheet == null)
        {
            sheet = Resources.Load<Sheet>($"{TableRootPath}/{SheetName}");
            if (sheet == null)
            {
                Debug.LogError($"NotExistTable: {SheetName}");
                return null;
            }
            sheet.OnLoaded();
            tables[SheetName] = sheet;
        }

        return sheet;
    }

    public static Sheet GetCompositeTable(Type compositeTableType, CompositeSheetAttribute compositeSheetAttribute)
    {
        string sheetName = compositeTableType.FullName;
        if (tables.TryGetValue(sheetName, out Sheet sheet) == false || sheet == null)
        {
            sheet = (Sheet)Activator.CreateInstance(compositeTableType);

            //foreach (Type subSheetType in compositeSheetAttribute.SheetsTypeList)
            //{
            //    Sheet subSheet = GetTable(subSheetType);
            //    foreach (Descriptor desc in )
            //    {
            //        sheet.AddObj(desc.Id, desc);
            //    }
            //}
            sheet.OnUpdated(compositeSheetAttribute.SheetsTypeList.SelectMany(s => GetTable(s).AllObj()).ToList(), new Dictionary<string, List<string>>());
            sheet.OnLoaded();
            tables[sheetName] = sheet;
        }

        return sheet;
    }
}
