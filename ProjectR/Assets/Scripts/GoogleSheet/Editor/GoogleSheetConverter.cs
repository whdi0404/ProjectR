using Sirenix.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace Table.Editor
{
    public static class GoogleSheetConverter
    {
        struct SheetReadDef
        {
            public List<string> ColumnNames { get; private set; }
            public Vector2Int StartPos { get; private set; }

            public SheetReadDef(List<string> columnNames, Vector2Int startPos)
            {
                ColumnNames = columnNames;
                StartPos = startPos;
            }
        }

        public static List<object> ConvertData(Type sheetType, IList<IList<object>> rawSheet, out Dictionary<string, List<string>> referenceInfo )
        {
            Type descriptorType = sheetType.GetBaseClasses().First().GetGenericArguments()[0];

            var allDescProperties = descriptorType.GetProperties();

            SheetReadDef sheetReadDef = GetSheetData(rawSheet);

            Dictionary<string, PropertyInfo> columnProperties = new Dictionary<string, PropertyInfo>();
            foreach (var property in allDescProperties)
            {
                GoogleColumnAttribute columnAttr = property.GetCustomAttribute<GoogleColumnAttribute>();

                if (columnAttr == null)
                    continue;

                string columnName = columnAttr.Name ?? property.Name;

                if (sheetReadDef.ColumnNames.Contains(columnName) == false)
                { 
                    Debug.LogError($"[{sheetType.Name}] Not Exist Column in GoogleSpreadSheet: {columnName}");
                    continue;
                }

                if (columnProperties.ContainsKey(columnName) == true )
                    throw new Exception($"[{sheetType.Name}] Exist Same GoogleColumnAttribute Property in Class: {columnName}");

                columnProperties.Add(columnName, property);
            }

            List<object> rows = new List<object>();
            HashSet<object> duplicateCheck = new HashSet<object>();

            referenceInfo = new Dictionary<string, List<string>>();

            for (int i = sheetReadDef.StartPos.x; i < rawSheet.Count; ++i)
            {
                Dictionary<string, List<string>> descReferenceInfoTemp = new Dictionary<string, List<string>>();
                object descInstance = Activator.CreateInstance(descriptorType);

                string descId = null;

                for (int j = sheetReadDef.StartPos.y; j < rawSheet[i].Count; ++j)
                {
                    string columnName = sheetReadDef.ColumnNames[j];
                    object value = rawSheet[i][j];
                    if (columnProperties.TryGetValue(columnName, out PropertyInfo property) == true)
                    {
                        string valueToString = Convert.ToString(value);
                        if (columnName == "Id")
                            descId = valueToString;

                        object convertedData = GoogleSheetConvert.ConvertValue(valueToString, property.PropertyType);

                        property.SetValue(descInstance, convertedData);
                    }

                    if (GoogleSheetConvert.IsReferenceColumn(columnName) == true)
                    {
                        descReferenceInfoTemp.Add(columnName, Convert.ToString(value).Split(',').ToList());
                    }
                }

                foreach (var reference in descReferenceInfoTemp)
                    referenceInfo.Add($"{descId}.{reference.Key}", reference.Value);

                var key = descriptorType.GetProperties()
                    .Last(s => s.Name == typeof(Descriptor).GetProperties()[0].Name)
                    .GetValue(descInstance);

                if (duplicateCheck.Contains(key) == true)
                {
                    Debug.LogError($"[{sheetType.Name}] Same Key Exist GoogleSpreadSheet: {key}");
                    continue;
                }

                rows.Add(descInstance);
                duplicateCheck.Add(key);
            }

            return rows;
        }

        private static SheetReadDef GetSheetData(IList<IList<object>> rawSheet)
        {
            var keyName = typeof(Descriptor).GetProperties()[0].Name;

            for (int i = 0; i < rawSheet.Count; ++i)
            {
                IList<object> row = rawSheet[i];
                for (int j = 0; j < row.Count; ++i)
                {
                    object cell = row[j];
                    if (Convert.ToString(cell) == keyName)
                    {
                        List<string> columnNames = row.Select(o => Convert.ToString(o)).ToList();
                        SheetReadDef sheetData = new SheetReadDef(columnNames, new Vector2Int(i + 1, j));

                        return sheetData;
                    }
                }
            }

            return default;
        }
    }
}
