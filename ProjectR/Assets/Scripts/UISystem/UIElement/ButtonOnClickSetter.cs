using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ButtonOnClickSetter : DataSetter<Button>
{
    public override void SetValue(Data data)
    {
        Data<UnityAction> eventData = data as Data<UnityAction>;
        if (eventData == null)
        {
            Debug.LogError("Invalid Data");
            return;
        }

        base.SetValue(data);

        uiBehaviour.onClick.RemoveAllListeners();
        uiBehaviour.onClick.AddListener(eventData.Value);
    }
}
