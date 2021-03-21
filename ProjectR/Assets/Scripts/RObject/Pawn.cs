using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : RObject
{
	private List<(Vector2Int, float)> path;
	public Vector2Int destination;

	private float currentTileWeight;
	public Pawn(DataContainer properties) : base(properties)
	{
		path = new List<(Vector2Int, float)>();
	}

	public void Move(Vector2Int destPos)
	{
		path.Clear();
		path = WorldManager.FindPath(MapTilePosition, destPos, path);
		//Todo: 현재 서있는 위치가 [0]->[1]로 가는 방향이라면 path[0] 삭제해야 함.
		currentTileWeight = path[0].Item2;
		WorldManager.onTileChanged += OnTileChanged;
	}

	public void OnTileChanged(Vector2Int changedPos, string prevTile, string newTile)
	{
		if (path.Exists(s => s.Item1 == changedPos) == true)
		{
			WorldManager.onTileChanged -= OnTileChanged;
			Move(path[path.Count - 1].Item1);
		}
	}

	public override void Update(float dt)
	{
		UpdateMove(dt);
	}

	private void UpdateMove(float dt)
	{
		if (path.Count == 0)
			return;

        if (Properties.TryGetValue("speed", out float moveDelta) == false)
            moveDelta = 1.0f;

        moveDelta *= currentTileWeight * dt;

		while (moveDelta > 0 && path.Count > 0)
		{
			Vector2 targetPosition = new Vector2(MapTilePosition.x + 0.5f, MapTilePosition.y + 0.5f);

			bool isMiddlePos = false;
			if (path[0].Item1 != MapTilePosition)
			{
				Vector2 toTarget = new Vector2(path[0].Item1.x + 0.5f, path[0].Item1.y + 0.5f) - targetPosition;
				toTarget.Normalize();
				Vector2 toTargetSign = new Vector2(Mathf.Approximately(toTarget.x, 0) ? 0 : Mathf.Sign(toTarget.x), Mathf.Approximately(toTarget.y, 0) ? 0 : Mathf.Sign(toTarget.y));

				targetPosition += toTargetSign * (0.5001f);
				isMiddlePos = true;
			}

			Vector2 toT = targetPosition - MapPosition;
			float remainDistance = toT.magnitude;

			if (remainDistance <= moveDelta)
			{
				MapPosition = targetPosition;
				moveDelta -= remainDistance;

				if (isMiddlePos == true)
				{
					currentTileWeight = path[0].Item2;
					continue;
				}
				else
				{
					path.RemoveAt(0);
					continue;
				}
			}
			else
			{
				MapPosition += toT.normalized * moveDelta;
				moveDelta = 0;
			}
		}

		if (path.Count == 0)
			WorldManager.onTileChanged -= OnTileChanged;
	}

    public override void VisualUpdate(float dt)
    {
    }
}