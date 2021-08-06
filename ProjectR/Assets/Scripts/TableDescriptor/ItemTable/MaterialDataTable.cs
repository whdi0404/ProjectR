using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class MaterialDataDescriptor : ItemDataDescriptor
{

}

[GoogleWorkSheet("ItemTable", "MaterialData")]
public class MaterialDataTable : Sheet<MaterialDataDescriptor>
{
}