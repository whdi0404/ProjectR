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

    public PlanObject(StructurePlanningDescriptor planningDesc)
    {
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

    public override void VisualUpdate(float dt)
    {

    }

    public void Work(float work)
    {
        RemainWorkload -= work;

        if (RemainWorkload <= 0)
        {
            //Todo: PlanningDesc로 물건 생성
            GameManager.Instance.DestroyRObject(this);
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
}