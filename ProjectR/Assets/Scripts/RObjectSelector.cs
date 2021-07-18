using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Command
{
    public string name;
    public Sprite sprite;
    public Action action;
}

public class RObjectSelector
{
    private HashSet<RObject> selectedObjectList = new HashSet<RObject>();
    public HashSet<RObject> SelectedObjectList { get => selectedObjectList; }

    private SmartDictionary<string, Command> commands = new SmartDictionary<string, Command>();

    public RObjectSelector()
    {
        //commands["Attack"] = ;
        //commands["AAA"] = ;
    }

    public bool Contains(RObject rObj)
    {
        return selectedObjectList.Contains(rObj);
    }

    public void AddRObject(RObject rObj)
    {
        selectedObjectList.Add(rObj);
        OnChangedSelectedObject();
    }

    public void RemoveRObject(RObject rObj)
    {
        selectedObjectList.Remove(rObj);
        OnChangedSelectedObject();
    }

    public void Clear()
    {
        selectedObjectList.Clear();
        OnChangedSelectedObject();
    }

    private void OnChangedSelectedObject()
    {
        //UIManager.Instance.RootContext.CommandPanel.SetCommands();
    }
}
