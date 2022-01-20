using Table;

public class ItemDataDescriptor : Descriptor
{
	[GoogleColumn]
	public string Image { get; set; }

	[GoogleColumn]
	public float Weight { get; set; }

	[GoogleColumn]
	public int StackAmount { get; set; }
}

[CompositeSheet(typeof(MaterialDataTable), typeof(WeaponDataTable), typeof(FoodDataTable))]
public class ItemDataTable : Sheet<ItemDataDescriptor>
{
}