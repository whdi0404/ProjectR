using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class WorkPlaceObject : RObject
{
    public abstract WorkBase GetWork(Pawn pawn);

    public abstract void OnCompleteWork(WorkBase work);

    public override void OnGUI()
    {
        base.OnGUI();
        var work = GetWork(null);

        Vector2 v = CameraManager.Instance.MainCamera.WorldToScreenPoint(this.MapPosition);
        v.y = Screen.height - v.y;

        foreach (var item in work.WorkHolder.GetRemainReqItemList())
        {
            GUI.Label(new Rect(v, new Vector2(200, 20)), $"{item.Amount}");
        }
    }
}


public abstract class WorkBase
{
    public WorkPlaceObject WorkPlace { get; protected set; }
    public WorkHolder WorkHolder { get; protected set; }

    public WorkBase(WorkPlaceObject workPlace, List<Item> requireItemList)
    {
        WorkPlace = workPlace;
        WorkHolder = GameManager.Instance.ItemSystem.CreateWorkHolder(workPlace, requireItemList);
    }

    public bool IsWorkable { get => WorkHolder.GetRemainReqItemList().FirstOrDefault().Amount == 0; }

    public float RemainWorkload { get; protected set; }

    public virtual bool Work(Pawn pawn)
    {
        float work = Time.deltaTime * 1.0f;//Todo: 폰 능력치에 따라 가중치 추가
        RemainWorkload -= work;

        if (RemainWorkload <= 0)
        {
            Complete();
            WorkHolder.RemoveAllItems();
            WorkPlace.OnCompleteWork(this);
            return true;
        }

        return false;
    }

    public abstract void Complete();

    public virtual void Cancel()
    {
        GameManager.Instance.ItemSystem.DestroyContainer(WorkHolder);
        GameManager.Instance.WorkSystem.ReserveSystem.RemoveAllReserverFromDest(this);
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