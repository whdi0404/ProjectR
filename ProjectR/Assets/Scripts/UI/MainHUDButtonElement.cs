using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MainHUDButtonElement : UIContext
{
    public Data<string> ButtonName { get; private set; } = new Data<string>();
    public Data<UnityAction> ButtonAction { get; private set; } = new Data<UnityAction>();
}
