using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GameManager : SingletonBehaviour<GameManager>
{
    public WorldMap WorldMap { get; private set; }

    private List<RObject> rObjList;
    private List<RObjectBehaviour> activedBehaviours;
    private List<RObjectBehaviour> deactivedBehaviours;


    protected override void Start()
    {
        GameObject go = new GameObject("WorldMap");
        WorldMap = go.AddComponent<WorldMap>();

        rObjList = new List<RObject>();
        activedBehaviours = new List<RObjectBehaviour>();
        deactivedBehaviours = new List<RObjectBehaviour>();
    }

    private void Update()
    {
        foreach (var rObj in rObjList)
            rObj.Update(Time.deltaTime);
    }
}