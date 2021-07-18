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

    private RObjectBehaviour behaviour;

    public virtual void Update(float dt)
    {
        if (behaviour == null && IsInCamera())
        {
            RObjectBehaviour bh = GOPoolManager.Instance.Pop("RObj").GetComponent<RObjectBehaviour>();
            bh.Init(this);

            behaviour = bh;
        }
        else if(behaviour != null && !IsInCamera())
        {
            behaviour.gameObject.SetActive(false);
            behaviour = null;
        }
    }

    internal void Destroy()
    {
        if (behaviour != null)
        { 
            behaviour.gameObject.SetActive(false);
            behaviour = null;
        }
    }

    public abstract void VisualUpdate(float dt);

    private bool IsInCamera()
    {
        Camera camera = Camera.main;

        Vector3 v = camera.WorldToViewportPoint(MapPosition);

        return v.x > 0 && v.x < 1 && v.y > 0 && v.y < 1;
    }
}