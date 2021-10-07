using Sirenix.Serialization;
using System;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class WorkBenchDataDescriptor : InstallObjectDataDescriptor
{
}

[GoogleWorkSheet("InstallObjectTable", "WorkbenchData")]
public class WorkBenchDataTable : Sheet<WorkBenchDataDescriptor>
{
}