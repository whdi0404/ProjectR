using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GameManager : SingletonBehaviour<GameManager>
{
    public WorldMap WorldMap { get; private set; }
    private List<RObject> rObjList = new List<RObject>();

    protected override void Start()
    {
        WorldMap = new GameObject("WorldMap").AddComponent<WorldMap>();

        GOPoolManager.Instance.Init("RObj", new GameObject("RObject", typeof(RObjectBehaviour)));
    }

    private void Update()
    {
        foreach (var rObj in rObjList)
            rObj.Update(Time.deltaTime);
    }

    public void CreateRObject(RObject rObj)
    {
        rObjList.Add(rObj);
    }

    public void DestroyRObject(RObject rObj)
    {
        rObj.Destroy();
        rObjList.Remove(rObj);
    }
}