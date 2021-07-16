using System;
using System.Collections.Generic;
using UnityEngine;
using M4u;
using Object = UnityEngine.Object;


//Singleton으로 변경 후, Canvas에 추가 하자
[Singleton(CreateInstance = true, DontDestroyOnLoad = true, PrefabPath = "")]
public class UIManager : SingletonBehaviour<UIManager>
{
    [SerializeField]
    private Transform uiRoot;
    public Transform UIRoot { get => uiRoot; }

    private SmartDictionary<Type, GameObject> cache = new SmartDictionary<Type, GameObject>();

    protected override void Awake()
    {
        base.Awake();
        GetComponent<M4uContextRoot>().Context = new UIRootContext();
    }

    public T CreateUIInstance<T>(int hierarchyPos = -1) where T : UIContext
    {
        var uiBase = cache[typeof(T)];
        if (uiBase == null)
        {
            var attr = typeof(T).GetCustomAttributes(typeof(UIPrefabAttribute), false);
            if (attr == null || attr.Length == 0)
            {
                Debug.LogError($"{typeof(T).Name} is Not Set UIPrefabAttribute");
                return null;
            }

            string prefabPath = (attr[0] as UIPrefabAttribute).PrefabPath;
            GameObject prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"{prefabPath} NotExist");
                return null;
            }

            uiBase = cache[typeof(T)] = prefab;
        }

        GameObject instance = Object.Instantiate(uiBase.gameObject);
        instance.transform.parent = uiRoot.transform;
        if (hierarchyPos >= 0)
            instance.transform.SetSiblingIndex(Math.Min(hierarchyPos, uiRoot.childCount - 1));
        instance.SetActive(true);

        return instance.GetComponent<T>();
    }

    public List<UIContext> GetCurrentUIList()
    {
        List<UIContext> uiList = new List<UIContext>();

        for (int i = 0; i < uiRoot.childCount; ++i)
        {
            if (uiRoot.GetChild(i).TryGetComponent(out UIContext uiContext) == true)
            {
                uiList.Add(uiContext);
            }
        }

        return uiList;
    }
}