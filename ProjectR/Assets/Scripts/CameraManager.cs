using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class CameraManager : SingletonBehaviour<CameraManager>
{
    [SerializeField]
    private Camera mainCamera;

    public Camera MainCamera { get => mainCamera; }

    private Vector2? dragStartPos;

    public void OnScroll(Vector2 mouseWorldPos, float weight)
    {
        Vector3 worldToScreenPos = mainCamera.WorldToScreenPoint(mouseWorldPos);

        mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - weight,1,20);
        mainCamera.transform.position = mouseWorldPos;
        Vector3 tempWorldPos = mainCamera.ScreenToWorldPoint(worldToScreenPos);
        mainCamera.transform.position += new Vector3(mouseWorldPos.x, mouseWorldPos.y) - tempWorldPos;
    }

    public void OnDrag(Vector2 mouseWorldPos)
    {
        if (dragStartPos == null)
            dragStartPos = mouseWorldPos;
        else if(dragStartPos != mouseWorldPos)
        {
            Vector2 vec2ToTarget = dragStartPos.Value - mouseWorldPos;
            mainCamera.transform.position += mainCamera.transform.TransformVector(new Vector3(vec2ToTarget.x, vec2ToTarget.y));
        }
    }
    public void OnDragEnd()
    {
        dragStartPos = null;
    }
}
