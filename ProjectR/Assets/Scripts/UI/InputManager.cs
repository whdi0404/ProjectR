using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public struct PickObject
{
    public RObjectBehaviour rObj;
    public AtlasInfoDescriptor tileDesc;
    public Vector2Int tilePos;
}

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class InputManager : SingletonBehaviour<InputManager>
{
    private RObjectSelector selector;
    //override event
    private System.Action<PickObject> overrideLeftButtonDownPick;
    private System.Action<PickObject> overrideLeftButtonUpPick;
    private System.Action<PickObject> overrideLeftButtonPick;
    public Vector2 CurrentMouseWorldPosition { get => Camera.main.ScreenToWorldPoint(Input.mousePosition); }

    public Vector2Int CurrentMouseTilePosition { get => new Vector2Int((int)CurrentMouseWorldPosition.x, (int)CurrentMouseWorldPosition.y); }

    protected override void Awake()
    {
        base.Awake();
        selector = new RObjectSelector();
    }

    void Update()
    {
        if (Input.GetMouseButton(2) == true)
        {
            CameraManager.Instance.OnDrag(CurrentMouseWorldPosition);
        }
        if (Input.GetMouseButtonUp(2) == true)
        {
            CameraManager.Instance.OnDragEnd();
        }

        if (Input.mouseScrollDelta != Vector2.zero)
        {
            CameraManager.Instance.OnScroll(CurrentMouseWorldPosition, Input.mouseScrollDelta.y);
        }

        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        if (Input.GetMouseButtonDown(0) == true)
        {
            PickObject pickObj = Pick(CurrentMouseWorldPosition);
            if (overrideLeftButtonDownPick != null)
                overrideLeftButtonDownPick(pickObj);
            else
                OnLeftMouseButtonDown(pickObj);
        }
        else if (Input.GetMouseButton(0) == true)
        {
            PickObject pickObj = Pick(CurrentMouseWorldPosition);
            if (overrideLeftButtonPick != null)
                overrideLeftButtonPick(pickObj);
            else
                OnLeftMouseButton(pickObj);
        }
        else if (Input.GetMouseButtonUp(0) == true)
        {
            PickObject pickObj = Pick(CurrentMouseWorldPosition);
            if (overrideLeftButtonUpPick != null)
                overrideLeftButtonUpPick(pickObj);
            else
                OnLeftMouseButtonUp(pickObj);
        }

        if (Input.GetMouseButtonDown(1) == true)
        {
            PickObject pickObj = Pick(CurrentMouseWorldPosition);

            foreach (var selectedObject in selector.SelectedObjectList)
            {
                Pawn pawn = selectedObject as Pawn;
                if (pawn != null)
                {
                    pawn.AI.Reset();
                }
            }
        }

        //TestCode
        if (Input.GetKey(KeyCode.Escape) == true)
        {
            PlanningManager.Instance.Cancel();
        }
    }

    public PickObject Pick(Vector2 worldPos)
    {
        PickObject pickObj = new PickObject();

        Vector2Int tilePos = new Vector2Int((int)worldPos.x, (int)worldPos.y);
        pickObj.tilePos = tilePos;

        RaycastHit2D hitInfo = Physics2D.Raycast(new Vector3(worldPos.x, worldPos.y, -100), Vector3.forward);
        if (hitInfo.transform != null)
        {
            pickObj.rObj = hitInfo.transform.GetComponent<RObjectBehaviour>();
        }
        if (GameManager.Instance.WorldMap.TryGetTile(tilePos, out AtlasInfoDescriptor tileDesc) == true)
        {
            pickObj.tileDesc = tileDesc;
        }

        return pickObj;
    }

    private void OnLeftMouseButtonDown(PickObject pickObj)
    {
        if (pickObj.rObj != null)
        {
            selector.Clear();
            selector.AddRObject(pickObj.rObj.RObj);
        }

        TestClick(pickObj);
    }

    private void OnLeftMouseButtonUp(PickObject pickObj)
    {

    }

    private void OnLeftMouseButton(PickObject pickObj)
    {

    }

    public void OverrideLeftMouseButtonDown(System.Action<PickObject> evt)
    {
        overrideLeftButtonDownPick = evt;
    }

    public void OverrideLeftMouseButtonUp(System.Action<PickObject> evt)
    {
        overrideLeftButtonUpPick = evt;
    }

    public void OverrideLeftMouseButton(System.Action<PickObject> evt)
    {
        overrideLeftButtonPick = evt;
    }

    //TestCode
    private void SpawnTestPawn(Vector2Int spawnPos)
    {
        Pawn pawn = new Pawn();
        pawn.MapTilePosition = spawnPos;
        GameManager.Instance.ObjectManager.CreateObject(pawn);
    }

    private void TestClick(PickObject pickObj)
    {
        if (Input.GetKey(KeyCode.A) == true)
        {
            SpawnTestPawn(CurrentMouseTilePosition);
        }

        if (Input.GetKey(KeyCode.S) == true)
        {
            var itemDesc = TableManager.GetTable<ItemDataTable>().Find("TestMaterial");
            GameManager.Instance.ObjectManager.ItemSystem.DropItem(CurrentMouseTilePosition, new Item(itemDesc, itemDesc.StackAmount / 2), out Item dropFailed);
        }

        if (Input.GetKey(KeyCode.D) == true)
        {
            if (pickObj.rObj != null)
            {
                GameManager.Instance.ObjectManager.DestroyObject(pickObj.rObj.RObj);
            }
        }
    }
}
