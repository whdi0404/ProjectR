using Sirenix.OdinInspector;
using Sirenix.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILayoutContents<T> : MonoBehaviour where T : UIContext
{
    [SerializeField]
    private Transform layout;
    public Transform Layout { get => layout; }

    [SerializeField]
    private T content;
    public T Content { get => content; }
    public List<T> InstanceList { get; private set; } = new List<T>();


    public void Start()
    {
        if (Layout == null)
            Debug.LogError("Layout Not Set");
        if (Content == null)
            Debug.LogError("Content Not Set");
    }

    public T CreateInstance()
    {
        GameObject go = Instantiate(Content.gameObject);
        go.SetActive(true);

        T newInstance = go.GetComponent<T>();

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

    public bool RemoveInstance(T instance)
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