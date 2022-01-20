using Table;

public class WeaponDataDescriptor : ItemDataDescriptor
{
}

[GoogleWorkSheet("ItemTable", "WeaponData")]
public class WeaponDataTable : Sheet<WeaponDataDescriptor>
{
}