using System.Collections.Generic;
using Table;

public class TestDescriptor : Descriptor
{
    [GoogleColumn("TestCol1")]
    public string TestCol1 { get; private set; }

    [GoogleColumn("TestCol2")]
    public string TestCol2 { get; private set; }

    [GoogleColumn]
    public string TestCol3 { get; private set; }

    [GoogleColumn]
    public string TestCol4 { get; private set; }

    [GoogleRefColumn("TestSheet.Sheet2", "Chidl1")]
    public List<TestChildDescriptor> child1 { get; private set; }
}

[GoogleWorkSheet("TestSheet","Sheet1")]
public class TestTable : Sheet<TestDescriptor>
{
}