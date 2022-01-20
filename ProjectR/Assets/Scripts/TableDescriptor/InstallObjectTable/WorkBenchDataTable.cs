using Table;

public class WorkBenchDataDescriptor : InstallObjectDataDescriptor
{
}

[GoogleWorkSheet("InstallObjectTable", "WorkbenchData")]
public class WorkBenchDataTable : Sheet<WorkBenchDataDescriptor>
{
}