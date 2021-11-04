using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapRenderer
{
	private WorldMap worldMap;

	private TileGroup[,] tileGroups;
	private Vector2Int tileGroupSize { get => worldMap.TileGroupSize; }
	private Vector2Int tileGroupAmount;

	private AtlasObject atlasObject;
	private Material tileGroupMaterial;

	private Camera mainCamera;

    public void Initialize(WorldMap worldMap, Vector2Int tileGroupAmount)
	{
		this.worldMap = worldMap;
		this.tileGroupAmount = tileGroupAmount;

		atlasObject = Resources.Load<AtlasObject>("Atlas/TileAtlasObject");
		tileGroupMaterial = Resources.Load<Material>("Materials/TileMaterial");

		mainCamera = Camera.main;

		tileGroups = new TileGroup[tileGroupAmount.x, tileGroupAmount.y];
        for (int x = 0; x < tileGroupAmount.x; ++x)
            for (int y = 0; y < tileGroupAmount.y; ++y)
            {
                GameObject go = new GameObject("TileGroup");
                TileGroup tileGroup = go.AddComponent<TileGroup>();
                tileGroup.Initialize(atlasObject, tileGroupMaterial, tileGroupSize.x, tileGroupSize.y, "Water");
                tileGroups[x, y] = tileGroup;
            }

		UpdateTileGroup(true);
	}
	private Vector2Int prevCenterGroupPos;

	public void Update()
    {
		UpdateTileGroup();
	}

	public void OnChangeTile(Vector2Int tile)
	{
		Vector2Int centerGroupPos = new Vector2Int((int)(mainCamera.transform.position.x / tileGroupSize.x), (int)(mainCamera.transform.position.y / tileGroupSize.y));

		Vector2Int groupIndex = worldMap.TilePosToGroupIndex(tile);

		Vector2Int offset = centerGroupPos + Vector2Int.one * -tileGroupAmount / 2;

		Vector2Int rendererIndex = groupIndex - offset;

		if (groupIndex.x < offset.x
			|| groupIndex.x >= offset.x + tileGroupAmount.x
			|| groupIndex.y < offset.y
			|| groupIndex.y >= offset.y + tileGroupAmount.y)
			return;

		int ix = groupIndex.x % tileGroupAmount.x;
		int iy = groupIndex.y % tileGroupAmount.y;

		Vector2Int fragmentTile = tile - groupIndex * worldMap.TileGroupSize;

        worldMap.TryGetTile(tile, out AtlasInfoDescriptor desc);

        tileGroups[ix, iy].SetTile(desc.Id, fragmentTile.x, fragmentTile.y);
        tileGroups[ix, iy].Apply();
    }

	private void UpdateTileGroup(bool first = false)
	{
		Vector2Int centerGroupPos = new Vector2Int((int)(mainCamera.transform.position.x / tileGroupSize.x), (int)(mainCamera.transform.position.y / tileGroupSize.y));

		if (first == false && prevCenterGroupPos == centerGroupPos)
			return;

		Vector2Int groupMovedVec = centerGroupPos - prevCenterGroupPos;

		Vector2Int groupAmountStartOffset = -tileGroupAmount / 2;
		for (int x = 0; x < tileGroupAmount.x; ++x)
			for (int y = 0; y < tileGroupAmount.y; ++y)
			{
				int osX = x + groupMovedVec.x;
				int osY = y + groupMovedVec.y;
				if (first == false && (osX >= 0 && osX < tileGroupAmount.x &&
					osY >= 0 && osY < tileGroupAmount.y))
					continue;

				Vector2Int groupPos = new Vector2Int(
				centerGroupPos.x + groupAmountStartOffset.x + x,
				centerGroupPos.y + groupAmountStartOffset.y + y
				);

				int ix = groupPos.x % tileGroupAmount.x;
				int iy = groupPos.y % tileGroupAmount.y;
				if (ix < 0)
					ix += tileGroupAmount.x;
				if (iy < 0)
					iy += tileGroupAmount.y;

				Vector2Int v = groupPos * tileGroupSize;

				BoundsInt groupBoundary = new BoundsInt();
				groupBoundary.SetMinMax(
					new Vector3Int(v.x, v.y, 0),
					new Vector3Int(v.x + tileGroupSize.x, v.y + tileGroupSize.y, 0));

				if (worldMap.TryGetTileFragments(groupPos, out var fragmentData) == true)
				{
					for (int gx = 0; gx < groupBoundary.size.x; ++gx)
						for (int gy = 0; gy < groupBoundary.size.y; ++gy)
						{
							var desc = fragmentData.fragmentData[gx + gy * tileGroupSize.x];
							tileGroups[ix, iy].SetTile(desc.Id, gx, gy);
						}
				}
				else
				{
					tileGroups[ix, iy].SetAllTile("Water");
				}

				tileGroups[ix, iy].Apply();
				tileGroups[ix, iy].transform.position = new Vector3(groupBoundary.xMin, groupBoundary.yMin);
			}

		prevCenterGroupPos = centerGroupPos;
	}
}
