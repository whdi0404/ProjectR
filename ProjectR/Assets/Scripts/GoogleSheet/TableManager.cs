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
        GoogleWorkSheetAttribute attr = (GoogleWorkSheetAttribute)(type.GetCustomAttributes(false)[0]);
        return GetTable($"{attr.SpreadSheetName}.{attr.WorkSheetName}");
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
}
