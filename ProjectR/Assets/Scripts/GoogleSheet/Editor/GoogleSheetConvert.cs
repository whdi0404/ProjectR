using System;
using System.Collections;
using System.Linq;
using System.Numerics;
using UnityEditor;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace Table.Editor
{
    public static class GoogleSheetConvert
    {
        public static object ConvertValue(string value, Type type)
        {
            try
            {
                if (value == "#N/A")
                    value = "";

                if (IsNumericType(type))
                    value = value?.Replace(",", "");

                if (type == typeof(Type))
                    return Type.GetType(value);

                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                    return UnityEditor.AssetDatabase.LoadAssetAtPath(value, type);

                if (typeof(IList).IsAssignableFrom(type))
                    return CreateListFromValue(value, type);

                if (typeof(UnityEngine.Vector2Int).IsAssignableFrom(type))
                    return CreateVectorFromValue(value, typeof(Vector2Int), typeof(int));

                if (typeof(UnityEngine.Vector3Int).IsAssignableFrom(type))
                    return CreateVectorFromValue(value, typeof(Vector3Int), typeof(int));

                if (typeof(UnityEngine.Vector2).IsAssignableFrom(type))
                    return CreateVectorFromValue(value, typeof(Vector2), typeof(float));

                if (typeof(UnityEngine.Vector3).IsAssignableFrom(type))
                    return CreateVectorFromValue(value, typeof(Vector3), typeof(float));

                if (type.IsEnum)
                {
                    return string.IsNullOrEmpty(value) ? Enum.GetValues(type).GetValue(0) : Enum.Parse(type, value, true);
                }

                if (type.IsValueType)
                {
                    return string.IsNullOrEmpty(value) ? GetDefaultValue(type) : Convert.ChangeType(value, type);
                }

                return Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogException(e);
                return null;
            }
        }
        
        public static bool IsNumericType(Type type)
        {   
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
                return Activator.CreateInstance(t);

            return null;
        }

        private static object CreateListFromValue(object value, Type type)
        {
            var list = Activator.CreateInstance(type) as IList;
            foreach (var e in Convert.ToString(value).Split(','))
            {
                list.Add(ConvertValue(e.Trim(), type.GetGenericArguments()[0]));
            }

            return list;
        }

        private static object CreateVectorFromValue(object value, Type type, Type valueType)
        {
            var values = Convert.ToString(value).Split('(', ',', ')');
            var args = values
                .Skip(1)
                .Take(values.Length - 2)
                .Select(v => Convert.ChangeType(v.Trim(), valueType))
                .ToArray();

            return Activator.CreateInstance(type, args);
        }

        public static string ConvertToStringValue(object value, Type type)
        {
            try
            {
                if (typeof(UnityEngine.Object).IsAssignableFrom(type))
                {
                    return AssetDatabase.GetAssetPath(value as UnityEngine.Object);
                }

                return Convert.ToString(value);
            }
            catch
            {
                return type.FullName;
            }
        }

        public static bool IsReferenceColumn(string columnName)
        {
            return columnName.Split('.').Length == 3;
        }
    }
}
