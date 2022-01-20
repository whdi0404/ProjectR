using Table;

public class FoodDataDescriptor : ItemDataDescriptor
{
}

[GoogleWorkSheet("ItemTable", "FoodData")]
public class FoodDataTable : Sheet<FoodDataDescriptor>
{
}