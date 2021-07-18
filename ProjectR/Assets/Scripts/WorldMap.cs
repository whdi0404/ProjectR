using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap : MonoBehaviour
{
	public class TileFragmentData
	{
		public BoundsInt bounds;
		public AtlasInfoDescriptor[] fragmentData;

		public bool Contains(Vector2Int vector)
		{
			return bounds.xMin <= vector.x && bounds.xMax > vector.x &&
			bounds.yMin <= vector.y && bounds.yMax > vector.y;
		}

		public bool Intersects(BoundsInt b)
        {
            Bounds boundsVec = new Bounds(bounds.center, bounds.size);
            return boundsVec.Intersects(new Bounds(b.center, b.size));
        }

		public bool TryGetTile(Vector2Int tilePos, out AtlasInfoDescriptor desc)
		{
			desc = null;
			if (Contains(tilePos) == true)
            {
                Vector2Int v = tilePos - new Vector2Int(bounds.min.x, bounds.min.y);
				desc = fragmentData[v.x + v.y * bounds.size.x];

				return true;
			}
			return false;
		}
	}

	//MapData
	private List<TileFragmentData> fragmentDataList = new List<TileFragmentData>();
	private WorldMapRenderer worldMapRenderer;

	private void Start()
	{
		TileFragmentData fragmentData = MakeIsland(new Vector2Int(32,32), 256, Random.Range(0, int.MaxValue));
		fragmentDataList.Add(fragmentData);

        worldMapRenderer = new WorldMapRenderer();
        worldMapRenderer.Initialize(this, new Vector2Int(64, 64), new Vector2Int(8, 8));
    }

    private void Update()
    {
		worldMapRenderer.Update();
	}

    private TileFragmentData MakeIsland(Vector2Int startPos, int mapSize, int seed)
	{
		AtlasInfoDescriptor[] islandData = new AtlasInfoDescriptor[mapSize * mapSize];

		float[,] noiseMap = new float[mapSize, mapSize];

		(Vector2Int, int)[] gradationPoints = new (Vector2Int, int)[20];

		int gradationMinSize = 20;
		int gradationMaxSize = 200;

		Random.InitState(seed);
		for (int i = 0; i < gradationPoints.Length; ++i)
		{
			Vector2Int pos;
			int size;
			do
			{
				size = Random.Range(gradationMinSize, gradationMaxSize);
				pos = new Vector2Int(Random.Range(size, mapSize - size), Random.Range(size, mapSize - size));
			}
			while (gradationPoints.FirstOrDefault(s => (s.Item1 - pos).magnitude < Mathf.Abs(s.Item2 - size)) != default);

			gradationPoints[i] = (pos, size);
		}

		int mul = 32;

		ImprovedPerlin perlin = new ImprovedPerlin();
		perlin.setSeed(seed);

		for (int y = 0; y < mapSize; ++y)
			for (int x = 0; x < mapSize; ++x)
			{
				float noise = ImprovedPerlin.to01(perlin.noise2((float)x / mapSize * mul, (float)y / mapSize * mul));

				float maxGradation = 0;
				foreach (var gradationPoint in gradationPoints)
				{
					float length = (new Vector2Int(x, y) - gradationPoint.Item1).magnitude;

					if (gradationPoint.Item2 > length)
						maxGradation = Mathf.Max(maxGradation, 1 - length / gradationPoint.Item2);
				}
				float gradationNoise = noise * maxGradation;

				if (gradationNoise < 0.2f)
					islandData[x + y * mapSize] = TableManager.GetTable<TileAtlasInfoTable>().Find("Water");
				else if (gradationNoise < 0.3f)
					islandData[x + y * mapSize] = TableManager.GetTable<TileAtlasInfoTable>().Find("Sand");
				else
					islandData[x + y * mapSize] = TableManager.GetTable<TileAtlasInfoTable>().Find("Ground");

				if (gradationNoise >= 0.2f && noise > 0.8f)
					islandData[x + y * mapSize] = TableManager.GetTable<TileAtlasInfoTable>().Find("Wall");
			}
		TileFragmentData fragData = new TileFragmentData();
		fragData.bounds = new BoundsInt();
		fragData.bounds.SetMinMax(new Vector3Int(startPos.x, startPos.y,0), new Vector3Int(startPos.x + mapSize, startPos.y + mapSize, 0));
		fragData.fragmentData = islandData;

		return fragData;
	}

	public bool TryGetTile(Vector2Int tilePos, out AtlasInfoDescriptor desc)
	{
		desc = null;
		foreach (var fragmentData in fragmentDataList)
		{
			if (fragmentData.TryGetTile(tilePos, out desc) == true)
				return true;
		}

		return false;
	}

	public IEnumerable<Vector2Int> GetAdjacentTiles(Vector2Int mapTilePos, bool includeDiagonal)
	{
		yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y);
		yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y);
		yield return new Vector2Int(mapTilePos.x, mapTilePos.y - 1);
		yield return new Vector2Int(mapTilePos.x, mapTilePos.y + 1);

		if (includeDiagonal == true)
		{
			yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y - 1);
			yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y + 1);
			yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y - 1);
			yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y + 1);
		}
	}
}
