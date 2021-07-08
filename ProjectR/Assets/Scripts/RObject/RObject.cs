using System;
using UnityEngine;

public abstract class RObject
{
    public DataContainer Properties { get; protected set; }

    public Vector2 MapPosition { get; set; }

    public Vector2Int MapTilePosition 
    { 
        get => new Vector2Int((int)MapPosition.x, (int)MapPosition.y); 
        set => MapPosition = value + new Vector2(0.5f, 0.5f);
    }

    public float Speed { get { return 5.0f; }}

    public virtual Sprite VisualImage { get; set; }

    protected RObject(DataContainer properties)
    {
        if (properties == null)
            throw new Exception("NoData");
        //주의: NoCopy
        Properties = properties;
    }

    public abstract void Update(float dt);
    public abstract void VisualUpdate(float dt);
}