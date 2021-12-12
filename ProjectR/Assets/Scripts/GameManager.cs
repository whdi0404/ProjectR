using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class GameManager : SingletonBehaviour<GameManager>
{
    private ulong uniqueIdCounter;

    public WorldMap WorldMap { get; private set; }
    public ObjectManager ObjectManager { get; private set; }
    public ItemSystem ItemSystem { get; private set; }
    public WorkSystem WorkSystem { get; private set; }


    protected override void Start()
    {
        uniqueIdCounter = 1;
        WorldMap = new GameObject("WorldMap").AddComponent<WorldMap>();
        ObjectManager = new ObjectManager();
        ItemSystem = new ItemSystem();
        WorkSystem = new WorkSystem();

        ObjectManager.AddListener(ItemSystem);
        ObjectManager.AddListener(WorkSystem);
        WorldMap.RegionSystem.AddListener(ObjectManager);

        GOPoolManager.Instance.Init("RObj", new GameObject("RObject", typeof(RObjectBehaviour)));

        DeveloperTool.Instance.On = true;
    }

    private void Update()
    {
        ObjectManager.Update();
    }

    private void LateUpdate()
    {
        ObjectManager.LateUpdate();
    }

    public ulong GetNewUID()
    {
        return ++uniqueIdCounter;
    }
}