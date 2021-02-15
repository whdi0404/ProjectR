using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer
{
	private Dictionary<string, object> data = new Dictionary<string, object>();

	public T TryGetValue<T>( string key )
	{
		if ( data.TryGetValue( key, out object value ) == true && value is T == true )
			return ( T )value;

		return default;
	}

	public void SetData<T>( string key, T value )
	{
		if ( data.ContainsKey( key ) == true )
			data[ key ] = value;
		else
			data.Add( key, value );
	}
	public bool IsExist( string key )
	{
		return data.ContainsKey( key ) == true;
	}

	public bool IsNoExist( params string[] keys )
	{
		if ( keys == null )
			return false;

		foreach ( var key in keys )
			if ( data.ContainsKey( key ) == false )
				return false;

		return true;
	}

	public Type GetTypeOfData( string key )
	{
		if ( data.TryGetValue( key, out object value ) == true )
			return value.GetType();

		return null;
	}
}

public abstract class RObject
{
	public DataContainer Properties { get; protected set; }

	public Vector2 MapPosition { get; set; }

	public Vector2Int MapTilePosition { get => new Vector2Int( ( int )MapPosition.x, ( int )MapPosition.y ); }

	protected RObject( DataContainer properties )
	{
		if ( properties == null )
			throw new Exception( "NoData" );
		//주의: NoCopy
		Properties = properties;
	}

	public abstract void Update( float dt );
}

public class Pawn : RObject
{
	private List<(Vector2Int, float)> path;
	public Vector2Int destination;

	private float currentTileWeight;
	public Pawn( DataContainer properties ) : base( properties )
	{
		path = new List<(Vector2Int, float)>();
	}

	public void Move( Vector2Int destPos )
	{
		path.Clear();
		path = WorldManager.FindPath( MapTilePosition, destPos, path );
		//Todo: 현재 서있는 위치가 [0]->[1]로 가는 방향이라면 path[0] 삭제해야 함.
		currentTileWeight = path[ 0 ].Item2;
		WorldManager.onTileChanged += OnTileChanged;
	}

	public void OnTileChanged( Vector2Int changedPos, string prevTile, string newTile )
	{
		if ( path.Exists( s => s.Item1 == changedPos ) == true )
		{
			WorldManager.onTileChanged -= OnTileChanged;
			Move( path[ path.Count - 1 ].Item1 );
		}
	}

	public override void Update( float dt )
	{
		UpdateMove( dt );
	}

	private void UpdateMove( float dt )
	{
		if ( path.Count == 0 )
			return;

		float moveDelta = Properties.TryGetValue<float>( "speed" );
		if ( moveDelta == default )
			moveDelta = 1.0f;

		moveDelta *= currentTileWeight * dt;

		while ( moveDelta > 0 && path.Count > 0 )
		{
			Vector2 targetPosition = new Vector2( MapTilePosition.x + 0.5f, MapTilePosition.y + 0.5f );

			bool isMiddlePos = false;
			if ( path[ 0 ].Item1 != MapTilePosition )
			{
				Vector2 toTarget = new Vector2( path[ 0 ].Item1.x + 0.5f, path[ 0 ].Item1.y + 0.5f ) - targetPosition;
				toTarget.Normalize();
				Vector2 toTargetSign = new Vector2( Mathf.Approximately( toTarget.x, 0 ) ? 0 : Mathf.Sign( toTarget.x ), Mathf.Approximately( toTarget.y, 0 ) ? 0 : Mathf.Sign( toTarget.y ) );

				targetPosition += toTargetSign * ( 0.5001f );
				isMiddlePos = true;
			}

			Vector2 toT = targetPosition - MapPosition;
			float remainDistance = toT.magnitude;

			if ( remainDistance <= moveDelta )
			{
				MapPosition = targetPosition;
				moveDelta -= remainDistance;

				if ( isMiddlePos == true )
				{
					currentTileWeight = path[ 0 ].Item2;
					continue;
				}
				else
				{
					path.RemoveAt( 0 );
					continue;
				}
			}
			else
			{
				MapPosition += toT.normalized * moveDelta;
				moveDelta = 0;
			}
		}

		if ( path.Count == 0 )
			WorldManager.onTileChanged -= OnTileChanged;
	}
}

public class RObjectBehaviour : MonoBehaviour
{
	public RObject RObj { get; set; }

	public void Init( RObject rObject )
	{
		RObj = rObject;
	}

	private void Update()
	{
		transform.position = WorldManager.MapPosToWorldPosition( RObj.MapPosition );
	}
}