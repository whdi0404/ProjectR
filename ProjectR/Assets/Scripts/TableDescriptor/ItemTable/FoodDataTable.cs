using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class FoodDataDescriptor : ItemDataDescriptor
{

}

[GoogleWorkSheet("ItemTable", "FoodData")]
public class FoodDataTable : Sheet<FoodDataDescriptor>
{
}