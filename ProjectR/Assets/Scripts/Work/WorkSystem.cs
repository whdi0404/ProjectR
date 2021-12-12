using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkReserveSystem : ReserveSystemBase<WorkReserver, Pawn, WorkBase>
{

}

public class WorkReserver : ReserverBase<Pawn, WorkBase>
{
    public WorkReserver(Pawn source, WorkBase dest) : base(source, dest)
    {
    }

    public override void Destroy()
    {
        GameManager.Instance.WorkSystem.ReserveSystem.RemoveReserver(this);
    }
}

public class WorkSystem : IObjectManagerListener
{
    public WorkReserveSystem ReserveSystem { get; private set; }

    private SmartDictionary<WorkPlaceObject, List<WorkBase>> workDict = new SmartDictionary<WorkPlaceObject, List<WorkBase>>();

    public WorkSystem()
    {
        ReserveSystem = new WorkReserveSystem();
    }

    private List<WorkBase> GetOrMakeContainerList(WorkPlaceObject parentObj)
    {
        if (workDict.TryGetValue(parentObj, out var containerList) == false)
        {
            containerList = new List<WorkBase>();
            workDict.Add(parentObj, containerList);
        }

        return containerList;
    }

    public List<WorkBase> GetWorkBase(WorkPlaceObject rObj)
    {
        return GetOrMakeContainerList(rObj);
    }

    public BuildWork CreateBuildWork(BuildPlanObject parentObj, StructurePlanningDescriptor planningDesc)
    {
        BuildWork work = new BuildWork(parentObj, planningDesc);
        GetOrMakeContainerList(parentObj).Add(work);

        return work;
    }

    public void DestroyWork(WorkBase work)
    {
        if (workDict.TryGetValue(work.WorkPlace, out List<WorkBase> workList))
        {
            int index = workList.IndexOf(work);
            if (index != -1)
            {
                work.Cancel();
                workList.RemoveAt(index);
            }
        }
    }

    public void OnCreateObject(RObject rObject)
    {
    }

    public void OnDestroyObject(RObject rObject)
    {
        WorkPlaceObject workPlace = rObject as WorkPlaceObject;
        if (workPlace == null)
            return;

        if (workDict.TryGetValue(workPlace, out List<WorkBase> workList))
        {
            foreach (var work in workList)
            {
                work.Cancel();
            }
            workDict.Remove(workPlace);
        }
    }
}
