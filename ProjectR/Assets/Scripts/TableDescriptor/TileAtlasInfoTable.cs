using System.Collections.Generic;
using Table;

public class AtlasInfoDescriptor : Descriptor
{
    [GoogleColumn]
    public int DrawOrder { get; private set; }
    [GoogleColumn]
    public float MoveWeight { get; private set; }
    [GoogleColumn]
    public bool IsBlock { get; private set; }
}

[GoogleWorkSheet("TileAtlasInfo", "AtlasInfo")]
public class TileAtlasInfoTable : Sheet<AtlasInfoDescriptor>
{
}