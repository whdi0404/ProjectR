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
    public event System.Action<PickObject> onLeftButtonDownPick;
    public event System.Action<PickObject> onLeftButtonUpPick;
    public event System.Action<PickObject> onLeftButtonPick;
    public Vector2 CurrentMouseWorldPosition { get => Camera.main.ScreenToWorldPoint(Input.mousePosition); }

    public Vector2Int CurrentMouseTilePosition { get => new Vector2Int((int)CurrentMouseWorldPosition.x, (int)CurrentMouseWorldPosition.y); }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        if (Input.GetMouseButtonDown(0) == true)
        {
            Vector3 worldPos = CurrentMouseWorldPosition;
            onLeftButtonDownPick?.Invoke(Pick(worldPos));
        }
        else if (Input.GetMouseButton(0) == true)
        {
            Vector3 worldPos = CurrentMouseWorldPosition;
            onLeftButtonPick?.Invoke(Pick(worldPos));
        }
        else if(Input.GetMouseButtonUp(0) == true)
        {
            Vector3 worldPos = CurrentMouseWorldPosition;
            onLeftButtonUpPick?.Invoke(Pick(worldPos));
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
}
