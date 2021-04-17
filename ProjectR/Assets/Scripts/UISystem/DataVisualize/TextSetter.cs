using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextSetter : DataSetter<TextMeshProUGUI>
{
    public override void SetValue(Data data)
    {
        this.uiBehaviour.text = data.ToString();
    }
}