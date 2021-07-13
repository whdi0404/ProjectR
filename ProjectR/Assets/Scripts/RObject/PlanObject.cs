using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanObject : RObject
{
    private StructurePlanningDescriptor planningDesc;
    public StructurePlanningDescriptor PlanningDesc { get => planningDesc; }

    public PlanObject(DataContainer dataContainer) : base(dataContainer)
    {
        dataContainer.TryGetValue("desc", out planningDesc);

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
}