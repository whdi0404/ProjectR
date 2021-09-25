using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum AITagSubject
{
    PickupItem,
    Build,
    Attack,
}

public class AITagSystem
{
    /// <summary>
    /// object: 파라미터
    /// </summary>
    private SmartDictionary<Pawn, List<(RObject, object)>> tag = new SmartDictionary<Pawn, List<(RObject, object)>>();
    private SmartDictionary<RObject, List<Pawn>> tagged = new SmartDictionary<RObject, List<Pawn>>();

    public void Tag( Pawn pawn, RObject rObj, object param = null )
    {
        if ( tag[ pawn ] == null )
            tag[ pawn ] = new List<(RObject, object)>();
        tag[ pawn ].Add( (rObj, param) );

        if ( tagged[ rObj ] == null )
            tagged[ rObj ] = new List<Pawn>();
        tagged[ rObj ].Add( pawn );
    }

    public void UnTag( Pawn pawn, RObject rObj )
    {
        if ( tag[ pawn ]?.RemoveAll( obj => obj.Item1 == rObj ) > 0 )
        {
            //Event
        }
        if ( tagged[ rObj ]?.Remove( pawn ) == true )
        {
            //Event
        }
    }

    public void UnTagAll( RObject rObj )
    {
        var taggedList = tagged[ rObj ];
        if ( taggedList == null )
            return;
        for ( int i = taggedList.Count - 1; i >= 0; --i )
        {
            var pawn = taggedList[ i ];
            UnTag( pawn, rObj );
        }
    }

    public void UntagAllTagOfPawn( Pawn pawn )
    {
        var tagList = tag[ pawn ];
        if ( tagList == null )
            return;

        for ( int i = tagList.Count - 1; i >= 0; --i )
        {
            var rObj = tagList[ i ].Item1;
            UnTag( pawn, rObj );
        }
    }

    public bool IsTagged( RObject rObj )
    {
        return tagged[ rObj ]?.Count > 0;
    }

    public List<(RObject, object)> GetTaggedObjectsOfPawn( Pawn pawn )
    {
        return tag[ pawn ];
    }
}