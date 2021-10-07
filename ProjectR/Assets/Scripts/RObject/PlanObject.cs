using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlanObject : RObject
{
    private StructurePlanningDescriptor planningDesc;
    public StructurePlanningDescriptor PlanningDesc { get => planningDesc; }

    public override Vector2Int Size { get => planningDesc.Size; }

    public Inventory Inventory { get; private set; }

    public PlanObject(StructurePlanningDescriptor planningDesc)
    {
        IndexId = "Plan/Build";
        this.planningDesc = planningDesc;

        Inventory = new Inventory();
        Inventory.SetWeightLimit(0);

        if (planningDesc.Structure != null)
        {
            VisualImage = Resources.Load<Sprite>("PlanningTextures/StructurePlan");
        }
        if (planningDesc.InstallObject != null)
        {
            VisualImage = Resources.Load<Sprite>(planningDesc.InstallObject.Image);
        }

        RemainWorkload = planningDesc.Workload;
    }

    public float RemainWorkload { get; private set; }

    public bool IsFilledMaterials
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

    public override void VisualUpdate(float dt)
    {

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
        GameManager.Instance.ObjectManager.DestroyObject(this);

        if (PlanningDesc.Structure != null)
            GameManager.Instance.WorldMap.SetTile(MapTilePosition, PlanningDesc.Structure);

        //if (planObject.PlanningDesc.InstallObject != null)
        //GameManager.Instance.CreateRObject(new InstallObject());
    }
}