using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class MaterialDataDescriptor : ItemDataDescriptor
{

}


public class WeaponDataDescriptor : ItemDataDescriptor
{

}


public class FoodDataDescriptor : ItemDataDescriptor
{

}

[GoogleWorkSheet("ItemTable", "MaterialData")]
public class MaterialDataTable : Sheet<MaterialDataDescriptor>
{
}
[GoogleWorkSheet("ItemTable", "WeaponData")]
public class WeaponDataTable : Sheet<WeaponDataDescriptor>
{
}
[GoogleWorkSheet("ItemTable", "FoodData")]
public class FoodDataTable : Sheet<FoodDataDescriptor>
{
}

[CompositeSheet(typeof(MaterialDataTable), typeof(WeaponDataTable), typeof(FoodDataTable))]
public class ItemDataTable : Sheet<ItemDataDescriptor>
{ }