using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class InstallObjectDataDescriptor : Descriptor
{
    [GoogleColumn]
    public Vector2Int Size { get; set; }
    [GoogleColumn]
    public string Image { get; set; }
    [GoogleColumn]
    public bool IsBlock { get; set; }
}

public class WorkBenchDataDescriptor : InstallObjectDataDescriptor
{
}

[GoogleWorkSheet("InstallObjectTable", "WorkbenchData")]
public class WorkBenchDataTable : Sheet<WorkBenchDataDescriptor>
{

}

[CompositeSheet(typeof(WorkBenchDataTable))]
public class InstallObjectTable : Sheet<InstallObjectDataDescriptor>
{

}