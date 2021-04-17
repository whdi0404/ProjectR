using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public abstract class DataSetter : MonoBehaviour
{
    private Data dt;

    [SerializeField]
    private string id;

    public string ID { get => id; }

    private void OnEnable()
    {
        UIContext uiBase = gameObject.GetComponentInParent<UIContext>();
        uiBase.AddObject(this);

        dt?.RegistSetter(this);
    }

    private void OnDisable()
    {
        UIContext uiBase = gameObject.GetComponentInParent<UIContext>();
        uiBase.RemoveObject(this);

        dt?.UnregistSetter(this);
    }

    public virtual void SetValue(Data data)
    {
        if (dt != data)
        {
            if (dt != null)
            {
                dt.UnregistSetter(this);
            }
            dt = data;
            dt?.RegistSetter(this);
        }
    }
}

public abstract class DataSetter<T> : DataSetter where T : UIBehaviour
{
    [SerializeField]
    protected T uiBehaviour;
}

