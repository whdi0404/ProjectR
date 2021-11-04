using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuildPlanObject : WorkPlaceObject
{
    private BuildWork buildWork;

    private StructurePlanningDescriptor planningDesc;

    public override Vector2Int Size { get => planningDesc.Size; }

    public BuildPlanObject(StructurePlanningDescriptor planningDesc)
    {
        IndexId = "Work/Build";
        this.planningDesc = planningDesc;
        buildWork = new BuildWork(this, planningDesc);

        if (planningDesc.Structure != null)
        {
            VisualImage = Resources.Load<Sprite>("PlanningTextures/StructurePlan");
        }
        if (planningDesc.InstallObject != null)
        {
            VisualImage = Resources.Load<Sprite>(planningDesc.InstallObject.Image);
        }
    }

    public override void VisualUpdate(float dt)
    {

    }

    public override void OnCompleteWork(WorkBase work)
    {
        GameManager.Instance.ObjectManager.DestroyObject(this);
    }

    public override WorkBase GetWork(Pawn pawn)
    {
        return buildWork;
    }
}