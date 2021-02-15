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

	private Dictionary<string, Tile> tilePalette = new Dictionary<string, Tile>();

	private Tilemap tileMap = null;


	public void Start()
	{
		tileMap = GetComponentInChildren<Tilemap>();
		Generate( 1024, Random.Range( 0, int.MaxValue ) );
		WorldManager.Init( this );
	}

	public void Generate( int mapSize, int seed )
	{
		tilePalette.Add( ground, Resources.Load<Tile>( "Tiles/Ground" ) );
		tilePalette.Add( sand, Resources.Load<Tile>( "Tiles/Sand" ) );
		tilePalette.Add( water, Resources.Load<Tile>( "Tiles/Water" ) );
		tilePalette.Add( wall, Resources.Load<Tile>( "Tiles/Wall" ) );

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

				if ( gradationNoise < 0.2f )
					tileMap[ x + y * mapSize ] = tilePalette[ water ];
				else if ( gradationNoise < 0.3f )
					tileMap[ x + y * mapSize ] = tilePalette[ sand ];
				else
					tileMap[ x + y * mapSize ] = tilePalette[ ground ];

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

	float IPathFinderGraph<Vector2Int>.GetTileMovableWeight( Vector2Int pos )
	{
		TileBase tile = tileMap.GetTile( new Vector3Int( pos.x, pos.y, 0 ) );
		if ( tile == tilePalette[ ground ]
			|| tile == tilePalette[ sand ] )
			return 1;

		return -1;
	}

	IEnumerable<Vector2Int> IPathFinderGraph<Vector2Int>.GetAdjacentTiles( Vector2Int pos )
	{
		yield return new Vector2Int( pos.x - 1, pos.y - 1 );
		yield return new Vector2Int( pos.x - 1, pos.y );
		yield return new Vector2Int( pos.x - 1, pos.y + 1 );
		yield return new Vector2Int( pos.x, pos.y - 1 );
		yield return new Vector2Int( pos.x, pos.y + 1 );
		yield return new Vector2Int( pos.x + 1, pos.y - 1 );
		yield return new Vector2Int( pos.x + 1, pos.y );
		yield return new Vector2Int( pos.x + 1, pos.y + 1 );
	}
}