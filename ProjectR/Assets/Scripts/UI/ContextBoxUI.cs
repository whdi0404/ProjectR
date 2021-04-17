using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextBoxUI : UIContext
{
    [SerializeField]
    private UILayoutContents ContextContents;

    public void AddContext(string text, UnityAction action)
    {
        UIContext context = ContextContents.CreateInstance();
        ContextBoxElement contextBoxElement = context as ContextBoxElement;
        contextBoxElement.Text.Value = text;
        contextBoxElement.ButtonAction.Value = action;
    }

    public void RemoveContext(int index)
    {
        ContextContents.RemoveInstance(index);
    }

    public void Clear()
    {
        ContextContents.Clear();
    }
}