using System;
using System.Collections.Generic;

public class DataContainer : SmartDictionary<string, object>, ICloneable
{
    public bool ContainsKey(params string[] keys)
    {
        if (keys == null)
            return false;

        foreach (var key in keys)
            if (ContainsKey(key) == false)
                return false;

        return true;
    }

    public bool TryGetValue<T>(string key, out T value)
    {
        value = default;

        if (base.TryGetValue(key, out object objValue) == true)
        {
            if (objValue is T)
            {
                value = (T)objValue;
                return true;
            }
            else
                return false;
        }

        return default;
    }

    public Type GetTypeOfData(string key)
    {
        if (TryGetValue(key, out object value) == true)
            return value.GetType();

        return null;
    }

    public DataContainer Clone(params string[] keys)
    {
        DataContainer clone = new DataContainer();

        foreach (var key in keys)
        {
            clone[key] = this[key];
        }

        return clone;
    }

    public object Clone()
    {
        DataContainer clone = new DataContainer();

        foreach (var key in clone.Keys)
        {
            clone[key] = this[key];
        }

        return clone;
    }
}
