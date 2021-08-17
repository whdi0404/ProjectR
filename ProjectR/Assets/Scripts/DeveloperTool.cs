using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Singleton(CreateInstance = true, DontDestroyOnLoad = true)]
public class DeveloperTool : SingletonBehaviour<DeveloperTool>
{
    public bool On { get; set; }

    void OnGUI()
    {
        if (On == true)
            return;

        Vector2Int tile = InputManager.Instance.CurrentMouseTilePosition;

        if (GameManager.Instance.WorldMap.RegionSystem.GetRegionFromTilePos(tile, out LocalRegion region) == true)
        {
            region.DrawOnGUI(Color.white, true);
        }
    }
}
