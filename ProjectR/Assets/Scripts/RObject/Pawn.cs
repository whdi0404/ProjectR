using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Pawn : RObject, IRegionListener
{
	public PawnAI AI { get; private set; }

	public Inventory Inventory { get; private set; }

	public float Speed { get { return 5.0f; } }

    public bool IsMoving { get => moving != null; }

    public Vector2Int MoveDestination { get; private set; }

    private List<(Vector2Int, float)> movingPath = new List<(Vector2Int, float)>();
    private IEnumerator moving;

    public Pawn()
    {
        AI = new PawnAI(this);
        Inventory = GameManager.Instance.ItemSystem.CreateInventory(this);
		Inventory.SetWeightLimit( 10.0f );
		VisualImage = Resources.Load<Sprite>("PawnTextures/pawn");
		IndexId = $"Pawn";//적,아군,동맹,동물 등 표시
	}

    public override void Init()
    {
        base.Init();
    }

    public override void Update(float dt)
	{
		base.Update(dt);

        if (moving?.MoveNext() == false)
            moving = null;

        AI.UpdateTick();
    }

	public override void VisualUpdate(float dt)
	{

	}

    public override void Destroy()
    {
        base.Destroy();
        GameManager.Instance.WorkSystem.ReserveSystem.RemoveAllReserverFromSource(this);
    }

    public bool SetMove(Vector2Int destPos)
    {
        moving = Move(destPos);
        movingPath.Clear();
        return moving.MoveNext();
    }

    private IEnumerator Move(Vector2Int destPos)
    {
        WorldMap worldMap = GameManager.Instance.WorldMap;
        if (worldMap.PathFinder.FindPath(worldMap, MapTilePosition, destPos, ref movingPath) == true)
        {
            MoveDestination = destPos;
            yield return null;
        }
        else
        {
            MoveDestination = MapTilePosition;
            yield break;
        }

        float remainTimeDelta = Time.deltaTime;

        for (int pathIndex = movingPath.Count > 1 ? 1 : 0; pathIndex < movingPath.Count; ++pathIndex)
        {
            var nextPosWeight = movingPath[pathIndex];

            Vector2 nextDestination = nextPosWeight.Item1 + new Vector2(0.5f, 0.5f);
            Vector2 movingDir = ((nextPosWeight.Item1 - MapTilePosition)).ToVec2Float().normalized;
            Vector2 toNextDestDir = (nextDestination - MapPosition).normalized;

            if (Mathf.Approximately(toNextDestDir.x, movingDir.x) == false
                || Mathf.Approximately(toNextDestDir.y, movingDir.y) == false)
            {
                Vector2 currentTilePos = MapTilePosition + new Vector2(0.5f, 0.5f);
                if (Vector2.Distance(nextDestination, MapPosition) > Vector2.Distance(nextDestination, currentTilePos))
                {
                    nextDestination = currentTilePos;
                }
            }

            while (MapPosition != nextDestination)
            {
                float remainDistance = Vector2.Distance(MapPosition, nextDestination);

                float speed = Speed / nextPosWeight.Item2;

                float moveDelta = speed * remainTimeDelta;

                remainDistance = remainDistance - moveDelta;

                if (remainDistance > 0)
                {
                    MapPosition += moveDelta * (nextDestination - MapPosition).normalized;
                    yield return null;
                    remainTimeDelta = Time.deltaTime;
                }
                else
                {
                    MapPosition = nextDestination;
                    remainTimeDelta = (-remainDistance) / speed;
                }
            }
        }

        yield return null;
    }

    public void OnRegionChange(List<LocalRegion> removedLocalRegions, List<LocalRegion> newLocalRegions)
    {
        if (IsMoving == true)
        {
            foreach (var removedRegion in removedLocalRegions)
            {
                foreach (var pos in movingPath)
                {
                    if (removedRegion.IsIn(pos.Item1) == true)
                    {
                        SetMove(MoveDestination);
                        return;
                    }
                }
            }
        }
    }

    public override void OnGUI()
    {
        base.OnGUI();

        Vector2 v = CameraManager.Instance.MainCamera.WorldToScreenPoint(this.MapPosition);
        v.y = Screen.height - v.y;

        foreach (var item in Inventory.GetItemList(true, true))
        {            
            GUI.Label(new Rect(v + new Vector2(0,0), new Vector2(200, 20))
                , $"h,p: {item.Value}");
        }

        foreach (var item in Inventory.GetItemList(false, false))
        {
            GUI.Label(new Rect(v + new Vector2(0, 20), new Vector2(200, 20))
                , $"current: {item.Value}");
        }
    }
}