using Table;

public class TestChildDescriptor : Descriptor
{
    [GoogleColumn("TestCol1")]
    public string TestCol1 { get; private set; }
}

[GoogleWorkSheet("TestSheet", "Sheet2")]
public class TestChildTable : Sheet<TestChildDescriptor>
{
}