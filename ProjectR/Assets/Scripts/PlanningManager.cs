using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Planning
{
    public abstract void LeftButtonDown(PickObject pickObject);

    public abstract void LeftButtonUp(PickObject pickObject);

    public abstract void LeftButton(PickObject pickObject);

    public abstract void Cancel();

    //юс╫ц
    public abstract void OnGUI();
}

public class StructurePlanning : Planning
{
    private Vector2Int startTilePos;
    private Vector2Int currentTilePos;
    private PlanObject planObj;
    private bool isStructure;

    private StructurePlanningDescriptor planDesc;

    public StructurePlanning(StructurePlanningDescriptor planDesc)
    {
        this.planDesc = planDesc;
        DataContainer dataContainer = new DataContainer();
        dataContainer.Add("desc", planDesc);

        isStructure = planDesc.Structure != null;

        if (isStructure == false)
        {
            planObj = new PlanObject(dataContainer);
        }
    }

    public override void LeftButtonDown(PickObject pickObject)
    {
        startTilePos = pickObject.tilePos;
    }

    public override void LeftButtonUp(PickObject pickObject)
    {
    }

    public override void LeftButton(PickObject pickObject)
    {
        currentTilePos = pickObject.tilePos;
        planObj.MapPosition = InputManager.Instance.CurrentMouseTilePosition;
    }

    public override void Cancel()
    {
        GameManager.Instance.DestroyRObject(planObj);
        planObj = null;
    }

    public override void OnGUI()
    {

    }
}


[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class PlanningManager : SingletonBehaviour<PlanningManager>
{

    public Planning plan;
    public bool makeSquare;
    private Vector2Int startPos;
    private Vector2Int currentPos;
    public void SetPlan(Planning plan)
    {
        if(this.plan != null)
            Cancel();
        InputManager.Instance.onLeftButtonDownPick += plan.LeftButtonDown;
        InputManager.Instance.onLeftButtonPick += plan.LeftButton;
        InputManager.Instance.onLeftButtonUpPick += plan.LeftButtonUp;
    }

    public void Cancel()
    {
        if (plan == null)
            return;
        plan.Cancel();
        InputManager.Instance.onLeftButtonDownPick -= plan.LeftButtonDown;
        InputManager.Instance.onLeftButtonPick -= plan.LeftButton;
        InputManager.Instance.onLeftButtonUpPick -= plan.LeftButtonUp;
    }

    public void OnGUI()
    {
        plan?.OnGUI();
    }
}