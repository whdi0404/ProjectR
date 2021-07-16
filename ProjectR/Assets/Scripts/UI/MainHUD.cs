using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[UIPrefab("UI/Prefab/ContextBox")]
public class MainHUD : UIContext
{
    [SerializeField]
    private UILayoutContents<MainHUDButtonElement> ContextContents;
    public void Start()
    {
        MainHUDButtonElement contextBoxElement = ContextContents.CreateInstance();
        contextBoxElement.ButtonName.Value = "";
        contextBoxElement.ButtonAction.Value = () => Debug.Log("");
    }
}
