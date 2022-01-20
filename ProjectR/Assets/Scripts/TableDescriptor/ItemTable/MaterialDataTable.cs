using Table;

public class MaterialDataDescriptor : ItemDataDescriptor
{
}

[GoogleWorkSheet("ItemTable", "MaterialData")]
public class MaterialDataTable : Sheet<MaterialDataDescriptor>
{
}