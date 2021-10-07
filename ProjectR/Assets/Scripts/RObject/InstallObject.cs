using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkbenchObject : RObject
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
}