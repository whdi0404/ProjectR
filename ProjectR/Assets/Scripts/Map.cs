using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public sealed class Map : MonoBehaviour, IPathFinderGraph<Vector2Int>
{
	public const string ground = "Ground";
	public const string sand = "Sand";
	public const string water = "Water";
	public const string wall = "Wall";

	private Tilemap tileMap = null;

	private Dictionary<string, Tile> tilePalette = new Dictionary<string, Tile>();

	public int MapSize { get; private set; }
	public HashSet<Vector2Int> PlantEnableTiles { get; set; } = new HashSet<Vector2Int>();

	public void Start()
	{
		tileMap = GetComponentInChildren<Tilemap>();
		Generate( 512, Random.Range( 0, int.MaxValue ) );
		WorldManager.Init( this );
	}

	public void Generate( int mapSize, int seed )
	{
		tilePalette.Add( ground, Resources.Load<Tile>( "Tiles/Ground" ) );
		tilePalette.Add( sand, Resources.Load<Tile>( "Tiles/Sand" ) );
		tilePalette.Add( water, Resources.Load<Tile>( "Tiles/Water" ) );
		tilePalette.Add( wall, Resources.Load<Tile>( "Tiles/Wall" ) );

		MapSize = mapSize;

		TileBase[] tileMap = new TileBase[ mapSize * mapSize ];
		float[,] noiseMap = new float[ mapSize, mapSize ];

		(Vector2Int, int)[] gradationPoints = new (Vector2Int, int)[ 20 ];

		int gradationMinSize = 20;
		int gradationMaxSize = 200;

		Random.InitState( seed );
		for ( int i = 0; i < gradationPoints.Length; ++i )
		{
			Vector2Int pos;
			int size;
			do
			{
				size = Random.Range( gradationMinSize, gradationMaxSize );
				pos = new Vector2Int( Random.Range( size, mapSize - size ), Random.Range( size, mapSize - size ) );
			}
			while ( gradationPoints.FirstOrDefault( s => ( s.Item1 - pos ).magnitude < Mathf.Abs( s.Item2 - size ) ) != default );

			gradationPoints[ i ] = ( pos, size );
		}

		int mul = 32;

		ImprovedPerlin perlin = new ImprovedPerlin();
		perlin.setSeed( seed );

		for ( int y = 0; y < mapSize; ++y )
			for ( int x = 0; x < mapSize; ++x )
			{
				float noise = ImprovedPerlin.to01( perlin.noise2( ( float )x / mapSize * mul, ( float )y / mapSize * mul ) );

				float maxGradation = 0;
				foreach ( var gradationPoint in gradationPoints )
				{
					float length = ( new Vector2Int( x, y ) - gradationPoint.Item1 ).magnitude;

					if ( gradationPoint.Item2 > length )
						maxGradation = Mathf.Max( maxGradation, 1 - length / gradationPoint.Item2 );
				}
				float gradationNoise = noise * maxGradation;

				if (gradationNoise < 0.2f)
					tileMap[x + y * mapSize] = tilePalette[water];
				else if (gradationNoise < 0.3f)
					tileMap[x + y * mapSize] = tilePalette[sand];
				else
                {
                    PlantEnableTiles.Add(new Vector2Int(x, y));
                    tileMap[x + y * mapSize] = tilePalette[ground];
				}

				if ( gradationNoise >= 0.2f && noise > 0.8f )
					tileMap[ x + y * mapSize ] = tilePalette[ wall ];
			}

		this.tileMap.SetTilesBlock( new BoundsInt( 0, 0, 0, mapSize, mapSize, 1 ), tileMap );
	}

	public string GetTile( Vector2Int tilePos )
	{
		return tileMap.GetTile( new Vector3Int( tilePos.x, tilePos.y, 0 ) )?.name;
	}

	public void SetTile( Vector2Int tilePos, string tileName )
	{
		PlantEnableTiles.Remove(tilePos);
		if (tileName == ground)
			PlantEnableTiles.Add(tilePos);
		tileMap.SetTile( new Vector3Int( tilePos.x, tilePos.y, 0 ), tilePalette[ tileName ] );
	}
	public Vector3 MapPositionToWorldPosition( Vector2 mapPosition )
	{
		return new Vector3(mapPosition.x * tileMap.cellSize.x, mapPosition.y * tileMap.cellSize.y );
	}

	public Vector2Int WorldPositionToMapTilePos( Vector3 worldPosition )
	{
		Vector3Int tilePos = tileMap.WorldToCell( new Vector3( worldPosition.x, worldPosition.y ) );
		return new Vector2Int( tilePos.x, tilePos.y );
	}

	public float GetTileMovableWeight( Vector2Int mapTilePos )
	{
		TileBase tile = tileMap.GetTile( new Vector3Int( mapTilePos.x, mapTilePos.y, 0 ) );
		if ( tile == tilePalette[ ground ]
			|| tile == tilePalette[ sand ] )
			return 1;

		return -1;
	}

	public IEnumerable<Vector2Int> GetAdjacentTiles( Vector2Int mapTilePos, bool includeDiagonal)
    {
        if (IsCorrectPosition(new Vector2Int(mapTilePos.x - 1, mapTilePos.y)) == true)
            yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y);
		if (IsCorrectPosition(new Vector2Int(mapTilePos.x + 1, mapTilePos.y)) == true)
			yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y);
		if (IsCorrectPosition(new Vector2Int(mapTilePos.x, mapTilePos.y - 1)) == true)
			yield return new Vector2Int(mapTilePos.x, mapTilePos.y - 1);
		if (IsCorrectPosition(new Vector2Int(mapTilePos.x, mapTilePos.y + 1)) == true)
			yield return new Vector2Int(mapTilePos.x, mapTilePos.y + 1);

		if(includeDiagonal == true)
		{
			if (IsCorrectPosition(new Vector2Int(mapTilePos.x - 1, mapTilePos.y - 1)) == true)
				yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y - 1);
			if (IsCorrectPosition(new Vector2Int(mapTilePos.x - 1, mapTilePos.y + 1)) == true)
				yield return new Vector2Int(mapTilePos.x - 1, mapTilePos.y + 1);
			if (IsCorrectPosition(new Vector2Int(mapTilePos.x + 1, mapTilePos.y - 1)) == true)
				yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y - 1);
			if (IsCorrectPosition(new Vector2Int(mapTilePos.x + 1, mapTilePos.y + 1)) == true)
				yield return new Vector2Int(mapTilePos.x + 1, mapTilePos.y + 1);
		}
    }
    public bool IsCorrectPosition(Vector2Int mapTilePos)
    {
		return mapTilePos.x >= 0 && mapTilePos.x < MapSize && mapTilePos.y >= 0 && mapTilePos.y < MapSize;
	}

	public bool IsPlantEnableTile(Vector2Int mapTilePos)
	{
		return PlantEnableTiles.Contains(mapTilePos);
	}
}