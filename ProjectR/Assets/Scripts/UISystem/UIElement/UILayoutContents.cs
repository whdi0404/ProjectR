using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILayoutContents : MonoBehaviour
{
    [SerializeField]
    private Transform layout;
    public Transform Layout { get => layout; }

    [SerializeField]
    private UIContext content;
    public UIContext Content { get => content; }
    public List<UIContext> InstanceList { get; private set; } = new List<UIContext>();


    public void Start()
    {
        if (Layout == null)
            Debug.LogError("Layout Not Set");
        if (Content == null)
            Debug.LogError("Content Not Set");
    }

    public UIContext CreateInstance()
    {
        GameObject go = Instantiate(Content.gameObject);
        go.SetActive(true);

        UIContext newInstance = go.GetComponent<UIContext>();

        InstanceList.Add(newInstance);
        go.transform.SetParent(Layout);

        return newInstance;
    }

    public bool RemoveInstance(int index)
    {
        if (InstanceList.Count <= index)
            return false;

        Destroy(InstanceList[index]);
        InstanceList.RemoveAt(index);
        return true;
    }

    public bool RemoveInstance(UIContext instance)
    {
        if (InstanceList.Remove(instance) == true)
        {
            Destroy(instance);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Clear()
    {
        foreach (var instance in InstanceList)
        {
            Destroy(instance);
        }
        InstanceList.Clear();
    }
}