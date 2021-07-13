using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class InstallObjectDataDescriptor : Descriptor
{
    [GoogleColumn]
    public Vector2Int Size { get; private set; }
    [GoogleColumn]
    public string Image { get; private set; }
    [GoogleColumn]
    public bool IsBlock { get; private set; }
}

public class WorkBenchDataDescriptor : InstallObjectDataDescriptor
{
}

[GoogleWorkSheet("InstallObjectTable", "WorkbenchData")]
public class WorkBenchDataTable : Sheet<WorkBenchDataDescriptor>
{

}

[CompositeSheet(typeof(InstallObjectDataDescriptor))]
public class InstallObjectTable : Sheet<InstallObjectDataDescriptor>
{

}