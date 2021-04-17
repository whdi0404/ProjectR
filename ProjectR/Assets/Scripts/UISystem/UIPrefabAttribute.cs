using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class UIPrefabAttribute : Attribute
{
    public string PrefabPath { get; private set; }

    public UIPrefabAttribute(string prefabPath)
    {
        PrefabPath = prefabPath;
    }
}
