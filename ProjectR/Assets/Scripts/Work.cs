using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class WorkPlaceObject : RObject
{
    public abstract WorkBase GetWork(Pawn pawn);

    public abstract void OnCompleteWork(WorkBase work);
}


public abstract class WorkBase
{
    public WorkPlaceObject WorkPlace { get; protected set; }
    public WorkHolder WorkHolder { get; protected set; }

    public WorkBase(WorkPlaceObject workPlace, List<Item> requireItemList)
    {
        WorkPlace = workPlace;
        WorkHolder = GameManager.Instance.ObjectManager.ItemSystem.CreateWorkHolder(workPlace, requireItemList);
    }

    public bool IsWorkable { get => WorkHolder.GetRemainReqItemList().Count == 0; }

    public float RemainWorkload { get; protected set; }

    public virtual bool Work(Pawn pawn)
    {
        float work = Time.deltaTime * 1.0f;//Todo: 폰 능력치에 따라 가중치 추가
        RemainWorkload -= work;

        if (RemainWorkload <= 0)
        {
            Complete();
            WorkPlace.OnCompleteWork(this);
            return true;
        }

        return false;
    }

    public abstract void Complete();

    public virtual void Cancel()
    {
        WorkHolder.OnDestroy();
    }
}

public class BuildWork : WorkBase
{
    public StructurePlanningDescriptor PlanningDesc { get; private set; }

    public BuildWork(WorkPlaceObject workPlace, StructurePlanningDescriptor planningDesc) : base(workPlace, planningDesc.ReqItemList)
    {
        PlanningDesc = planningDesc;
    }

    public override void Complete()
    {
        if (PlanningDesc.Structure != null)
            GameManager.Instance.WorldMap.SetTile(WorkPlace.MapTilePosition, PlanningDesc.Structure);
        else if (PlanningDesc.InstallObject != null)
            GameManager.Instance.ObjectManager.CreateObject(InstallObject.CreateInstallObject(PlanningDesc.InstallObject));
    }
}