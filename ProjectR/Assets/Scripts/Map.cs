using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Map : MonoBehaviour
{
	[SerializeField]
	private Tilemap tileMap;

	public Tilemap TIleMap { get => tileMap;  }

	public void Start()
	{
		Generate( 1024, Random.Range( 0, int.MaxValue ) );
	}

	public void Generate( int mapSize, int seed )
	{
		Tile groundTile = Resources.Load<Tile>( "Tiles/Ground" );
		Tile sandTile = Resources.Load<Tile>( "Tiles/Sand" );
		Tile waterTile = Resources.Load<Tile>( "Tiles/Water" );
		Tile wallTile = Resources.Load<Tile>( "Tiles/Wall" );

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
					tileMap[ x + y * mapSize ] = waterTile;
				else if ( gradationNoise < 0.3f )
					tileMap[ x + y * mapSize ] = sandTile;
				else
					tileMap[ x + y * mapSize ] = groundTile;

				if( gradationNoise >= 0.2f && noise > 0.8f )
					tileMap[ x + y * mapSize ] = wallTile;
			}

		this.tileMap.SetTilesBlock( new BoundsInt( 0, 0, 0, mapSize, mapSize, 1 ), tileMap );
	}
}