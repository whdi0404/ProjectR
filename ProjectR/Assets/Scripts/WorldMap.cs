using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldMap : MonoBehaviour, IPathFinderGraph<Vector2Int>
{
	public Vector2Int TileGroupSize { get; private set; } = new Vector2Int(64, 64);

	public class TileFragmentData
	{
		public BoundsInt bounds;
		public AtlasInfoDescriptor[] fragmentData;

		public TileFragmentData(BoundsInt bounds)
		{
			this.bounds = bounds;
			fragmentData = new AtlasInfoDescriptor[bounds.size.x * bounds.size.y];

			AtlasInfoDescriptor waterDesc = TableManager.GetTable<TileAtlasInfoTable>().Find("Water");

			for (int i = 0; i < fragmentData.Length; ++i)
				fragmentData[i] = waterDesc;
		}

		public bool Intersects(BoundsInt b)
        {
			return bounds.Intersects(b);
        }

		public bool TryGetTile(Vector2Int tilePos, out AtlasInfoDescriptor desc)
		{
			desc = null;
			if (bounds.TileContains(tilePos) == true)
            {
                Vector2Int v = tilePos - new Vector2Int(bounds.min.x, bounds.min.y);
				desc = fragmentData[v.x + v.y * bounds.size.x];

				return true;
			}
			return false;
		}

		public bool TrySetTile(Vector2Int tilePos, AtlasInfoDescriptor desc)
		{
			if (bounds.TileContains(tilePos) == true)
			{
				Vector2Int v = tilePos - new Vector2Int(bounds.min.x, bounds.min.y);
				fragmentData[v.x + v.y * bounds.size.x] = desc;

				return true;
			}
			return false;
		}
	}

	//MapData
	private SmartDictionary<Vector2Int, TileFragmentData> tileGroupDict = new SmartDictionary<Vector2Int, TileFragmentData>();
	private WorldMapRenderer worldMapRenderer;
	public RegionSystem RegionSystem { get; private set; }

	public IEnumerable<Vector2Int> ExistGroupList { get => tileGroupDict.Keys; }

	public PathFinder<Vector2Int> PathFinder { get; private set; } = new PathFinder<Vector2Int>((a, b) => VectorExt.Get8DirectionLength(a, b));

	private void Start()
	{
		MakeIsland(new Vector2Int(32,32), 256, Random.Range(0, int.MaxValue));

        worldMapRenderer = new WorldMapRenderer();
        worldMapRenderer.Initialize(this, new Vector2Int(8, 8));

		RegionSystem = new RegionSystem();
		RegionSystem.Initialize(this);

		Bounds b = new Bounds();
		b.SetMinMax(new Vector3(0, 0), new Vector3(1, 1));
		Bounds b2 = new Bounds();
		b2.SetMinMax(new Vector3(0, 1), new Vector3(1, 2));

		Debug.Log(b.Intersects(b2));
	}

    private void Update()
    {
		worldMapRenderer.Update();
	}

    private void MakeIsland(Vector2Int startPos, int mapSize, int seed)
	{
		AtlasInfoDescriptor[] islandData = new AtlasInfoDescriptor[mapSize * mapSize];

		float[,] noiseMap = new float[mapSize, mapSize];

		(Vector2Int, int)[] gradationPoints = new (Vector2Int, int)[20];

		int gradationMinSize = 10;
		int gradationMaxSize = 50;

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

				Vector2Int pos = startPos + new Vector2Int(x, y);

				if (gradationNoise < 0.2f)
					SetTile(pos, TableManager.GetTable<TileAtlasInfoTable>().Find("Water"), false);
				else if (gradationNoise < 0.3f)
					SetTile(pos, TableManager.GetTable<TileAtlasInfoTable>().Find("Sand"), false);
				else
					SetTile(pos, TableManager.GetTable<TileAtlasInfoTable>().Find("Ground"), false);

				if (gradationNoise >= 0.2f && noise > 0.8f)
					SetTile(pos, TableManager.GetTable<TileAtlasInfoTable>().Find("Wall"), false);
			}
	}

	public bool TryGetTile(Vector2Int tilePos, out AtlasInfoDescriptor desc)
	{
		desc = null;

		Vector2Int groupIndex = TilePosToGroupIndex(tilePos);

		if (tileGroupDict.TryGetValue(groupIndex, out TileFragmentData fragmentData) == true)
		{
			if (fragmentData.TryGetTile(tilePos, out desc) == true)
				return true;
		}

		return false;
	}

	public bool TryGetTileFragments(Vector2Int groupIndex, out TileFragmentData fragmentData)
	{
		if (tileGroupDict.TryGetValue(groupIndex, out fragmentData) == true)
		{
			return true;
		}

		return false;
	}

	public float GetTileMovableWeight(Vector2Int pos)
	{
		if (TryGetTile(pos, out var desc))
		{
			if (desc.MoveWeight == 0)
				return 0;

			return 1.0f / desc.MoveWeight;
		}

		return 0;
	}

	public IEnumerable<(Vector2Int, float)> GetMovableAdjacentTiles(Vector2Int mapTilePos, bool includeDiagonal)
	{
		Vector2Int left = new Vector2Int(mapTilePos.x - 1, mapTilePos.y);
		Vector2Int right = new Vector2Int(mapTilePos.x + 1, mapTilePos.y);
		Vector2Int down = new Vector2Int(mapTilePos.x, mapTilePos.y - 1);
		Vector2Int up = new Vector2Int(mapTilePos.x, mapTilePos.y + 1);

		float leftWeight = GetTileMovableWeight(left);
		float rightWeight = GetTileMovableWeight(right);
		float downWeight = GetTileMovableWeight(down);
		float upWeight = GetTileMovableWeight(up);

        if (leftWeight > 0)
            yield return (left, leftWeight);

		if (rightWeight > 0)
			yield return (right, rightWeight);

		if (downWeight > 0)
			yield return (down, downWeight);

		if (upWeight > 0)
			yield return (up, upWeight);

		if (includeDiagonal == true)
        {
            if (leftWeight > 0 && downWeight > 0)
            {
                Vector2Int pos = new Vector2Int(mapTilePos.x - 1, mapTilePos.y - 1);
                float weight = GetTileMovableWeight(pos);
                if (weight > 0)
                    yield return (pos, weight);
            }

			if (leftWeight > 0 && upWeight > 0)
			{
				Vector2Int pos = new Vector2Int(mapTilePos.x - 1, mapTilePos.y + 1);
				float weight = GetTileMovableWeight(pos);
				if (weight > 0)
					yield return (pos, weight);
			}

			if (rightWeight > 0 && downWeight > 0)
			{
				Vector2Int pos = new Vector2Int(mapTilePos.x + 1, mapTilePos.y - 1);
				float weight = GetTileMovableWeight(pos);
				if (weight > 0)
					yield return (pos, weight);
			}

			if (rightWeight > 0 && upWeight > 0)
			{
				Vector2Int pos = new Vector2Int(mapTilePos.x + 1, mapTilePos.y + 1);
				float weight = GetTileMovableWeight(pos);
				if (weight > 0)
					yield return (pos, weight);
			}
		}
	}

	public void SetTile(Vector2Int pos, AtlasInfoDescriptor tileDesc, bool recreateRegion = true)
	{
		Vector2Int groupIndex = TilePosToGroupIndex(pos);

		if (tileGroupDict.TryGetValue(groupIndex, out TileFragmentData fragmentData) == true)
        {
            fragmentData.TrySetTile(pos, tileDesc);
        }
        else
        {
            Vector2Int groupStartPos = groupIndex * TileGroupSize;
            Vector2Int groupEndPos = groupStartPos + TileGroupSize;
            BoundsInt bounds = new BoundsInt();
            bounds.SetMinMax(new Vector3Int(groupStartPos.x, groupStartPos.y, 0), new Vector3Int(groupEndPos.x, groupEndPos.y, 0));

            tileGroupDict[groupIndex] = new TileFragmentData(bounds);
        }

		if (recreateRegion == true)
		{
			RegionSystem.CalculateLocalRegion(groupIndex);
		}
	}

	public Vector2Int TilePosToGroupIndex(Vector2Int pos)
	{
		return new Vector2Int(pos.x / TileGroupSize.x, pos.y / TileGroupSize.y);
	}
}