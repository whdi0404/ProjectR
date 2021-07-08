using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMapRenderer
{
	private WorldMap worldMap;

	private TileGroup[,] tileGroups;
	private Vector2Int tileGroupSize;
	private Vector2Int groupAmount;

	private AtlasObject atlasObject;
	private Material tileGroupMaterial;

	private Camera mainCamera;

    public void Initialize(WorldMap worldMap, Vector2Int tileGroupSize, Vector2Int groupAmount)
	{
		this.worldMap = worldMap;
		this.tileGroupSize = tileGroupSize;
		this.groupAmount = groupAmount;

		atlasObject = Resources.Load<AtlasObject>("Atlas/TileAtlasObject");
		tileGroupMaterial = Resources.Load<Material>("Materials/TileMaterial");

		mainCamera = Camera.main;

		tileGroups = new TileGroup[groupAmount.x, groupAmount.y];
        for (int x = 0; x < groupAmount.x; ++x)
            for (int y = 0; y < groupAmount.y; ++y)
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

	private void UpdateTileGroup(bool first = false)
	{
		Vector2Int centerGroupPos =
				   new Vector2Int((int)(mainCamera.transform.position.x / tileGroupSize.x), (int)(mainCamera.transform.position.y / tileGroupSize.y));

		if (first == false && prevCenterGroupPos == centerGroupPos)
			return;

		Vector2Int groupMovedVec = centerGroupPos - prevCenterGroupPos;

		Vector2Int groupAmountStartOffset = -groupAmount / 2;
		for (int x = 0; x < groupAmount.x; ++x)
			for (int y = 0; y < groupAmount.y; ++y)
			{
				int osX = x + groupMovedVec.x;
				int osY = y + groupMovedVec.y;
				if (first == false && (osX >= 0 && osX < groupAmount.x &&
					osY >= 0 && osY < groupAmount.y))
					continue;

				Vector2Int groupPos = new Vector2Int(
				centerGroupPos.x + groupAmountStartOffset.x + x,
				centerGroupPos.y + groupAmountStartOffset.y + y
				);

				int ix = groupPos.x % groupAmount.x;
				int iy = groupPos.y % groupAmount.y;
				if (ix < 0)
					ix += groupAmount.x;
				if (iy < 0)
					iy += groupAmount.y;

				Vector2Int v = groupPos * tileGroupSize;

				BoundsInt groupBoundary = new BoundsInt();
				groupBoundary.SetMinMax(
					new Vector3Int(v.x, v.y, 0),
					new Vector3Int(v.x + tileGroupSize.x, v.y + tileGroupSize.y, 0));

				for (int gx = 0; gx < groupBoundary.size.x; ++gx)
					for (int gy = 0; gy < groupBoundary.size.y; ++gy)
					{
						Vector2Int realPos = new Vector2Int(gx + groupBoundary.xMin, gy + groupBoundary.yMin);

						if (worldMap.TryGetTile(realPos, out AtlasInfoDescriptor desc) == true)
							tileGroups[ix, iy].SetTile(desc.Id, gx, gy);
						else
							tileGroups[ix, iy].SetTile("Water", gx, gy);
					}
				tileGroups[ix, iy].Apply();
				tileGroups[ix, iy].transform.position = new Vector3(groupBoundary.xMin, groupBoundary.yMin);
			}

		prevCenterGroupPos = centerGroupPos;
	}
}
