using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ContextBoxElement : UIContext
{
    public Data<string> Text { get; private set; } = new Data<string>();
    public Data<UnityAction> ButtonAction { get; private set; } = new Data<UnityAction>();

    public void Start()
    {
        this["Text"].SetValue(Text);
        this["ButtonClick"].SetValue(ButtonAction);
    }
}
