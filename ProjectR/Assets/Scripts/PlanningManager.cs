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

        isStructure = planDesc.Structure != null;

        planObj = new PlanObject(planDesc);
    }

    public override void LeftButtonDown(PickObject pickObject)
    {
        startTilePos = pickObject.tilePos;
    }

    public override void LeftButtonUp(PickObject pickObject)
	{
        for ( int x = Mathf.Min( currentTilePos.x, startTilePos.x ); x <= Mathf.Max( currentTilePos.x, startTilePos.x ); ++x )
        {
            for ( int y = Mathf.Min( currentTilePos.y, startTilePos.y ); y <= Mathf.Max( currentTilePos.y, startTilePos.y ); ++y )
			{
				var newPlanObj = new PlanObject( planDesc );
				newPlanObj.MapTilePosition = new Vector2Int( x, y );
				GameManager.Instance.ObjectManager.CreateObject( newPlanObj );
			}
        }
	}

    public override void LeftButton(PickObject pickObject)
    {
        currentTilePos = pickObject.tilePos;
        planObj.MapPosition = InputManager.Instance.CurrentMouseTilePosition;
    }

    public override void Cancel()
    {
        GameManager.Instance.ObjectManager.DestroyObject(planObj);
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
        InputManager.Instance.OverrideLeftMouseButtonDown(plan.LeftButtonDown);
        InputManager.Instance.OverrideLeftMouseButton(plan.LeftButton);
        InputManager.Instance.OverrideLeftMouseButtonUp(plan.LeftButtonUp);
    }

    public void Cancel()
    {
        if (plan == null)
            return;
        plan.Cancel();
        InputManager.Instance.OverrideLeftMouseButtonDown(null);
        InputManager.Instance.OverrideLeftMouseButton(null);
        InputManager.Instance.OverrideLeftMouseButtonUp(null);
    }

    public void OnGUI()
    {
        plan?.OnGUI();
    }
}