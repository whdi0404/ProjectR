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

[CompositeSheet(typeof(WorkBenchDataTable))]
public class InstallObjectTable : Sheet<InstallObjectDataDescriptor>
{
}