using UnityEngine;

public static class InstallObject
{
	public static RObject CreateInstallObject(InstallObjectDataDescriptor desc)
	{
		if (desc is WorkBenchDataDescriptor)
			return new WorkbenchObject(desc as WorkBenchDataDescriptor);

		return null;
	}
}

public class WorkbenchObject : WorkPlaceObject
{
	public WorkBenchDataDescriptor Desc { get; private set; }

	public Inventory Inventory { get; private set; }

	public override Vector2Int Size { get => Desc.Size; }

	public WorkbenchObject(WorkBenchDataDescriptor desc)
	{
		Desc = desc;
		VisualImage = Resources.Load<Sprite>(Desc.Image);
		IndexId = $"WorkBench/{Desc.Id}";
	}

	public override void VisualUpdate(float dt)
	{
	}

	public void Cancel()
	{
	}

	public void Complete()
	{
	}

	public void Work()
	{
	}

	public override void OnCompleteWork(WorkBase work)
	{
	}

	public override WorkBase GetWork(Pawn pawn)
	{
		return null;
	}
}