using System.Collections;
using System.Collections.Generic;
using Table;
using UnityEngine;

public class WeaponDataDescriptor : ItemDataDescriptor
{

}

[GoogleWorkSheet("ItemTable", "WeaponData")]
public class WeaponDataTable : Sheet<WeaponDataDescriptor>
{
}