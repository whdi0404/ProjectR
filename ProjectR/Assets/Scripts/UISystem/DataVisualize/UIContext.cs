using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIContext : MonoBehaviour
{
    public DataSetter this[string id]
    { 
        get => GetDataSetter(id);
    }

    private SmartDictionary<string, DataSetter> setters;

    public void AddObject(DataSetter setter)
    {
        setters.Add(setter.tag, setter);
    }

    public void RemoveObject(DataSetter setter)
    {
        setters.Remove(setter.tag);
    }

    public DataSetter GetDataSetter(string id)
    {
        return setters[id];
    }
}