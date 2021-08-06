using System;
using System.Collections.Generic;
using UnityEngine;

public struct NearNode : IComparable<NearNode>
{
    public Vector2Int position;
    public float distance;

    public int CompareTo(NearNode other)
    {
        return distance.CompareTo(other.distance);
    }
}

public abstract class RObject
{
    public ulong UniqueId { get; set; } = 0;

    public Vector2 MapPosition { get; set; }

    public virtual Vector2Int Size { get => new Vector2Int(1, 1); }

    public Vector2Int MapTilePosition
    { 
        get => new Vector2Int((int)MapPosition.x, (int)MapPosition.y); 
        set => MapPosition = value + new Vector2(0.5f, 0.5f);
    }

    public virtual Sprite VisualImage { get; set; }

    protected RObject()
    {
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

    public IEnumerable<Vector2Int> GetNearestAdjecentTile(Vector2Int position, bool containInside)
    {
        PriorityQueue<NearNode> queue = new PriorityQueue<NearNode>();

        if (containInside)
        {
            for (int x = 0; x < Size.x; ++x)
                for (int y = 0; y < Size.y; ++y)
                {
                    Vector2Int tile = MapTilePosition + new Vector2Int(x, y);
                    NearNode nearNode = new NearNode 
                    {
                        position = tile,
                        distance = Vector2Int.Distance(tile, position) 
                    };
                    queue.Enqueue(nearNode);
                }
        }


        for (int x = 0; x < Size.x; ++x)
        {
            Vector2Int tile = MapTilePosition + new Vector2Int(x, -1);
            NearNode nearNode = new NearNode
            {
                position = tile,
                distance = Vector2Int.Distance(tile, position)
            };
            queue.Enqueue(nearNode);

            tile = MapTilePosition + new Vector2Int(x, Size.y + 1);
            nearNode = new NearNode
            {
                position = tile,
                distance = Vector2Int.Distance(tile, position)
            };
            queue.Enqueue(nearNode);
        }

        for (int y = 0; y < Size.y; ++y)
        {
            Vector2Int tile = MapTilePosition + new Vector2Int(0, y);
            NearNode nearNode = new NearNode
            {
                position = tile,
                distance = Vector2Int.Distance(tile, position)
            };
            queue.Enqueue(nearNode);

            tile = MapTilePosition + new Vector2Int(Size.x, y);
            nearNode = new NearNode
            {
                position = tile,
                distance = Vector2Int.Distance(tile, position)
            };
            queue.Enqueue(nearNode);
        }

        while (queue.Count > 0)
        {
            yield return queue.Dequeue().position;
        }
    }

    public bool IsAdjeceny(RObject rObject)
    {
        Bounds bounds1 = new Bounds();
        Vector2Int bounds1Max = MapTilePosition + Size;
        bounds1.SetMinMax(new Vector3Int(MapTilePosition.x, MapTilePosition.y, 0), new Vector3Int(bounds1Max.x, bounds1Max.y, 1));
        bounds1.min -= new Vector3(0.1f, 0.1f);
        bounds1.max += new Vector3(0.1f, 0.1f);

        Bounds bounds2 = new Bounds();
        Vector2Int bounds2Max = rObject.MapTilePosition + rObject.Size;
        bounds2.SetMinMax(new Vector3Int(rObject.MapTilePosition.x, rObject.MapTilePosition.y, 0), new Vector3Int(bounds2Max.x, bounds2Max.y, 1));

        return bounds1.Intersects(bounds2);
    }
}