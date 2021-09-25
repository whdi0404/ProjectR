using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectManagerListener
{
    public void OnCreateObject( RObject rObject );
    public void OnDestroyObject( RObject rObject );
}

public class ObjectManagingSystem : IRegionListener
{
    private SmartDictionary<ulong, RObject> objects = new SmartDictionary<ulong, RObject>();
    private SmartDictionary<LocalRegion, SmartDictionary<ulong, RObject>> objectsOnRegion = new SmartDictionary<LocalRegion, SmartDictionary<ulong, RObject>>();

    private event Action<RObject> onCreateObjectEvent;
    private event Action<RObject> onDestroyObjectEvent;

    public ObjectManagingSystem()
    {
        
    }

	public void AddListener( IObjectManagerListener listener )
	{
		onCreateObjectEvent += listener.OnCreateObject;
		onDestroyObjectEvent += listener.OnDestroyObject;
	}
	public void RemoveListener( IObjectManagerListener listener )
	{
		onCreateObjectEvent -= listener.OnCreateObject;
        onDestroyObjectEvent -= listener.OnDestroyObject;
    }

	public void OnRegionChange( List<LocalRegion> removedLocalRegions, List<LocalRegion> newLocalRegions )
	{
		SmartDictionary<LocalRegion, SmartDictionary<ulong, RObject>> newRegions = new SmartDictionary<LocalRegion, SmartDictionary<ulong, RObject>>();

		foreach ( var removedRegion in removedLocalRegions )
        {
            foreach(var obj in objectsOnRegion[removedRegion])
            {
                foreach ( var localRegion in newLocalRegions )
                {
                    localRegion.
                }
                obj.Value
            }
        }
    }

    public void OnMovedObject( RObject rObject, Vector2Int prevPos, Vector2Int newPos )
    {
        GameManager.Instance.WorldMap.RegionSystem.GetRegionFromTilePos(prevPos, out LocalRegion prevRegion );
        GameManager.Instance.WorldMap.RegionSystem.GetRegionFromTilePos(newPos, out LocalRegion newRegion );

        if ( prevRegion == newRegion )
            return;

		objectsOnRegion[ prevRegion ].Remove( rObject.UniqueId );
		objectsOnRegion[ newRegion ].Add( rObject.UniqueId, rObject );
    }

    public void AddObject(RObject rObject)
    { 
    
    }

    public void RemoveObject(RObject rObject)
    {

    }
}