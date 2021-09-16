using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRegionListener
{
    public void OnRegionChange(Vector2Int tileGroupIndex);
}

public interface IRObjectListener
{
    public void OnCreateObject(RObject rObject);
    public void OnDestroyObject(RObject rObject);
}

public class WorkSystem : IRegionListener, IRObjectListener
{
    private Dictionary<LocalRegion, List<WorkPlace>> workPlaces;

    public void OnRegionChange(Vector2Int tileGroupIndex, List<LocalRegion> newRegions)
    {
        GameManager.Instance.RObjList
    }
    public void OnCreateObject(RObject rObject)
    {
        throw new System.NotImplementedException();
    }

    public void OnDestroyObject(RObject rObject)
    {
        throw new System.NotImplementedException();
    }
}

public class WorkPlace
{
    public enum Direction
    { 
        Left = 1 << 0,
        Top = 1 << 1,
        Right = 1 << 2,
        Bottom = 1 << 3,
    }

    private List<Work> workList = new List<Work>();
}

public abstract class Work
{
    public Work()


    public abstract bool IsWorkable { get; }

    public float RemainWorkload { get; protected set; }

    public void Cancel()
    {

    }
    public virtual void Work(float work)
    {

    }

    public virtual void Complete()
    { 
    
    }
}

public class BuildWork : Work
{
    public StructurePlanningDescriptor PlanningDesc { get { return planningDesc; } }

    private StructurePlanningDescriptor planningDesc;
    public BuildWork(StructurePlanningDescriptor planningDesc)
    {
        this.planningDesc = planningDesc;
    }

    public Inventory Inventory { get; private set; }

    public override bool IsWorkable
    {
        get
        {
            foreach (var reqItem in planningDesc.ReqItemList)
            {
                if (Inventory.GetItemAmount(reqItem.ItemDesc) < reqItem.Amount)
                    return false;
            }

            return true;
        }
    }

    public void Work(float work)
    {
        RemainWorkload -= work;

        if (RemainWorkload <= 0)
        {
            Complete();
        }
    }

    public List<RequireItem> GetRemainReqItemList()
    {
        List<RequireItem> reqItemList = new List<RequireItem>();
        foreach (var reqItem in planningDesc.ReqItemList)
        {
            int remainAmount = reqItem.Amount - Inventory.GetItemAmount(reqItem.ItemDesc);
            if (remainAmount > 0)
                reqItemList.Add(new RequireItem() { ItemDesc = reqItem.ItemDesc, Amount = remainAmount });
        }

        return reqItemList;
    }

    public void Complete()
    {
        if (PlanningDesc.Structure != null)
            GameManager.Instance.WorldMap.SetTile(MapTilePosition, PlanningDesc.Structure);

        //if (planObject.PlanningDesc.InstallObject != null)
        //GameManager.Instance.CreateRObject(new InstallObject());
    }
}