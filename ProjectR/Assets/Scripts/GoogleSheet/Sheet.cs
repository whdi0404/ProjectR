using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Table
{
    public abstract class Descriptor
    {
        [GoogleColumn]
        public string Id { get; protected set; }

        //public virtual void OnLoaded()
        //{
        //    Type type = GetType();

        //    var properties = type.GetProperties().Where(p=>p.GetCustomAttribute<GoogleRefColumnAttribute>() != null);


        //    foreach(var prop in properties)
        //    {
        //        GoogleRefColumnAttribute attr = prop.GetCustomAttribute<GoogleRefColumnAttribute>();
        //        if (ReferenceInfo.TryGetValue($"{attr.SheetName}.{attr.ColumnName}", out List<string> referenceIds) == false)
        //        {
        //            continue;
        //        }
        //        Sheet sheet = TableManager.GetTable(attr.SheetName);

        //        Type retType = sheet.GetType().GetProperty("Id").GetType();

        //        foreach (var id in referenceIds)
        //        {
        //            object obj = sheet.Find(id);
        //        }
        //    }
        //}
    }

    public class Sheet : SerializedScriptableObject
    {
        public virtual object FindObj(string key)
        {
            return null;
        }

        public virtual void OnUpdated(List<object> descriptorList, Dictionary<string, List<string>> referenceInfo)
        {

        }

        public virtual void OnLoaded()
        {
        }
    }

    public abstract class Sheet<TDescriptor> : Sheet where TDescriptor : Descriptor
    {
        [OdinSerialize]
        private Dictionary<string, TDescriptor> data;

        [OdinSerialize]
        private Dictionary<string, List<string>> referenceInfo;

        public override void OnUpdated(List<object> descriptors, Dictionary<string, List<string>> referenceInfo)
        {
            data = new Dictionary<string, TDescriptor>();
            foreach (var obj in descriptors)
            {
                TDescriptor desc = obj as TDescriptor;

                data.Add(desc.Id, desc);
            }

            this.referenceInfo = referenceInfo;
        }

        public override void OnLoaded()
        {
            if (referenceInfo?.Count > 0)
                LoadReferenceColumn();
        }

        private void LoadReferenceColumn()
        {
            foreach (var desc in data.Values)
            {
                foreach (var property in typeof(TDescriptor).GetProperties())
                {
                    GoogleRefColumnAttribute attr = property.GetCustomAttribute<GoogleRefColumnAttribute>();
                    if (attr == null)
                        continue;
                    referenceInfo.TryGetValue($"{desc.Id}.{attr.SheetName}.{attr.ColumnName}", out List<string> referenceIds);

                    var sheetTable = TableManager.GetTable(attr.SheetName);
                    Type refDescType = sheetTable.GetType().BaseType.GetGenericArguments()[0];

                    Type listType = typeof(List<>).MakeGenericType(refDescType);
                    object list = Activator.CreateInstance(listType);

                    foreach (var referenceId in referenceIds)
                    {
                        object obj = sheetTable.FindObj(referenceId);

                        listType.GetMethod("Add").Invoke(list, new object[] { obj });
                    }

                    property.SetValue(desc, list);
                }
            }
        }

        public virtual TDescriptor Find(string key)
        {
            data.TryGetValue(key, out TDescriptor value);
            return value;
        }

        public IEnumerable<TDescriptor> All()
        {
            return data.Values;
        }

        public IEnumerable<string> AllKeys()
        {
            return data.Keys;
        }
        public override object FindObj(string key)
        {
            return Find(key);
        }
    }
}