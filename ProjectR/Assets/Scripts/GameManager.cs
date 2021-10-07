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

    private Dictionary<AITagSubject, AITagSystem> tagSystemDict = new Dictionary<AITagSubject, AITagSystem>();

    protected override void Start()
    {
        uniqueIdCounter = 1;
        WorldMap = new GameObject("WorldMap").AddComponent<WorldMap>();
        ObjectManager = new ObjectManager();
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

    public AITagSystem GetAITagSystem(AITagSubject subject)
    {
        if (tagSystemDict.TryGetValue(subject, out AITagSystem aiTagSystem) == false)
            tagSystemDict.Add(subject, aiTagSystem = new AITagSystem());

        return aiTagSystem;
    }

    public bool FindPath(Vector2Int start, Vector2Int dest, ref List<(Vector2Int, float)> path, int maxSearch = 10000)
    {
        return WorldMap.PathFinder.FindPath(WorldMap, start, dest, ref path, maxSearch);
    }
    public bool IsReachable(Vector2Int start, Vector2Int dest)
    {
        return WorldMap.RegionSystem.IsReachable(start, dest);
    }

    public ulong GetNewUID()
    {
        return uniqueIdCounter++;
    }
}