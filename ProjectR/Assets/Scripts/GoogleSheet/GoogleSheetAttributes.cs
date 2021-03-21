using Sirenix.Serialization;
using System;

namespace Table
{
    [AttributeUsage(AttributeTargets.Class)]
    public class GoogleWorkSheetAttribute : OdinSerializeAttribute
    {
        public string SpreadSheetName { get; private set; }
        public string WorkSheetName { get; private set; }

        public GoogleWorkSheetAttribute()
        {
        }

        public GoogleWorkSheetAttribute(string spreadSheetName, string workSheetName)
        {
            SpreadSheetName = spreadSheetName;
            WorkSheetName = workSheetName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GoogleColumnAttribute : OdinSerializeAttribute
    {
        public string Name { get; private set; }

        public GoogleColumnAttribute()
        {
        }

        public GoogleColumnAttribute(string name)
        {
            this.Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class GoogleRefColumnAttribute : Attribute
    {
        public string SheetName { get; private set; }
        public string ColumnName { get; private set; }

        public GoogleRefColumnAttribute(string sheetName, string columnName)
        {
            SheetName = sheetName;
            ColumnName = columnName;
        }
    }
}