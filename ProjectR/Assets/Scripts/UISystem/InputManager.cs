using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (EventSystem.current.IsPointerOverGameObject() == true)
            return;

        if (Input.GetMouseButtonDown(0) == true)
        {
            Camera camera = Camera.main;

            RaycastHit2D hitInfo = Physics2D.Raycast(camera.transform.position, Vector3.forward);
            if (hitInfo.transform != null)
            {
                RObjectBehaviour rObj = hitInfo.transform.GetComponent<RObjectBehaviour>();
                //rObj.RObj
            }
            else
            {
                Vector3 worldPos = camera.ScreenToWorldPoint(Input.mousePosition);

                if(GameManager.Instance.WorldMap.TryGetTile(new Vector2Int((int)worldPos.x, (int)worldPos.y), out AtlasInfoDescriptor tileDesc) == true)
            }
        }
    }
}
