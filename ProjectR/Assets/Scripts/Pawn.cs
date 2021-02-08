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
	public DataContainer Properties { get; set; }

	public RObject( DataContainer dataContainer )
	{
		if ( dataContainer == null )
			throw new Exception( "NoData" );
		//주의: NoCopy
		Properties = dataContainer;
	}
	public abstract void Update();
}

public class Pawn : RObject
{
	public Pawn( DataContainer dataContainer ) : base( dataContainer )
	{
		if ( dataContainer.IsExist( "" ) == false )
			throw new Exception("NoData");
	}
	public override void Update()
	{
		throw new NotImplementedException();
	}
}

public class RObjectBehaviour : MonoBehaviour
{
	public RObject RObj { get; set; }

	public void Init( RObject rObject)
	{
		RObj = rObject;
	}

	public void Update()
	{
		RObj.Update();
	}
}