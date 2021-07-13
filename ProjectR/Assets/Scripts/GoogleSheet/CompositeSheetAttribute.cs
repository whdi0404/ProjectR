using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class CompositeSheetAttribute : Attribute
{
    public Type[] SheetsTypeList { get; private set; }

    public CompositeSheetAttribute()
    {
    }

    public CompositeSheetAttribute(params Type[] sheetsTypeList)
    {
        SheetsTypeList = sheetsTypeList;
    }
}