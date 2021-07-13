using System;
using System.Collections;
using System.Collections.Generic;
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
        GoogleWorkSheetAttribute workSheetAttr = (GoogleWorkSheetAttribute)(type.GetCustomAttributes(false)[0]);
        if(workSheetAttr != null)
            return GetTable($"{workSheetAttr.SpreadSheetName}.{workSheetAttr.WorkSheetName}");

        CompositeSheetAttribute compositeSheetAttr = (CompositeSheetAttribute)(type.GetCustomAttributes(false)[0]);
        if (workSheetAttr != null)
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
            foreach (Type subSheetType in compositeSheetAttribute.SheetsTypeList)
            {
                Sheet subSheet = GetTable(subSheetType);
                foreach (Descriptor desc in subSheet.AllObj())
                {
                    sheet.AddObj(desc.Id, desc);
                }
            }
            tables[sheetName] = sheet;
        }

        return sheet;
    }
}
