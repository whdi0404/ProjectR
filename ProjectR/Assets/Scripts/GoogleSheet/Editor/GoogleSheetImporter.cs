using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Sirenix.Utilities;
using UnityEngine;
using System.IO;
using UnityEditor;
using Table.Editor;

namespace Table
{
    public class GoogleSheetImporter : SerializedScriptableObject
    {
        [Serializable]
        public class GoogleSpreadSheetInfo
        {
            [TableColumnWidth(350)]
            [OdinSerialize]
            public string SheetId { get; private set; }

            [HideInInspector]
            [SerializeField]
            public string SheetName { get; private set; }

            [TableColumnWidth(120)]
            [Button("$SheetName"), VerticalGroup("Function")]
            public void GetInfo()
            {
                if (string.IsNullOrEmpty(SheetId) == false)
                    SheetName = GoogleSheetAPI.GetSpreadSheet(SheetId).Properties.Title;
            }

            [TableColumnWidth(120)]
            [Button("Open"), VerticalGroup("Function")]
            public void Open()
            {
                Application.OpenURL($"https://docs.google.com/spreadsheets/d/{SheetId}/edit#gid=0");
            }

            [TableColumnWidth(120)]
            [Button("Update"), VerticalGroup("Function")]
            public void Update()
            {
                List<Type> sheetTypes = AssemblyUtilities.GetTypes(AssemblyTypeFlags.CustomTypes)
                    .Where(t =>
                    {
                        if (t.IsClass == false
                            || typeof(ScriptableObject).IsAssignableFrom(t) == false
                            || t.InheritsFrom(typeof(Sheet<>)) == false)
                            return false;

                        GoogleWorkSheetAttribute worksheetAttr = t.GetCustomAttribute<GoogleWorkSheetAttribute>();
                        return worksheetAttr?.SpreadSheetName == SheetName;
                    }).ToList();

                foreach (var sheetType in sheetTypes)
                {
                    Update(sheetType);
                }

                //string[] allTableAssetGUIDs = AssetDatabase.FindAssets("t:Sheet", new[] { "Assets/Resources/Tables" });

                //foreach (var guid in allTableAssetGUIDs)
                //{
                //    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                //    AssetDatabase.LoadAssetAtPath<Sheet>(assetPath).OnLoaded();
                //}
            }

            private string Update(Type sheetType)
            {
                GoogleWorkSheetAttribute workSheetAttribute = sheetType.GetCustomAttribute<GoogleWorkSheetAttribute>();

                IList<IList<object>> rawSheet = GoogleSheetAPI.GetSpreadSheetData(SheetId, workSheetAttribute.WorkSheetName);
                List<object> descriptorList = GoogleSheetConverter.ConvertData(sheetType, rawSheet, out Dictionary<string, List<string>> referenceInfo);

                Sheet sheet = (Sheet)ScriptableObject.CreateInstance(sheetType);
                sheet.OnUpdated(descriptorList, referenceInfo);

                string assetPath = $"Assets/Resources/{TableManager.TableRootPath}/{workSheetAttribute.SpreadSheetName}.{workSheetAttribute.WorkSheetName}.asset";
                if (AssetDatabase.LoadAssetAtPath<Sheet>(assetPath) != null)
                {
                    AssetDatabase.DeleteAsset(assetPath);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.GenerateUniqueAssetPath(assetPath);
                AssetDatabase.CreateAsset(sheet, assetPath);
                AssetDatabase.Refresh();

                return assetPath;
            }
        }

        [OdinSerialize]
        [TableList(ShowIndexLabels = true)]
        public List<GoogleSpreadSheetInfo> SpreadSheetInfoList { get; private set; }
    }
}