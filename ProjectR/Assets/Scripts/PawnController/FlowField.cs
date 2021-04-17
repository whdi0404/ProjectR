using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowField
{
    private class FlowFieldCell
    {
        public Vector2Int position;
        public Vector2 direction;
        public float distance;

        public bool unpassable;

        public FlowFieldCell(Vector2Int position)
        {
            this.position = position;
        }
    }

    private Map map;
    private Vector2Int[] goals;
    private Dictionary<Vector2Int, FlowFieldCell> flowFieldCells = new Dictionary<Vector2Int, FlowFieldCell>();

    public FlowField(Map map)
    {
        this.map = map;
    }

    public bool Calculate(Vector2Int[] goals)
    {
        this.goals = goals;
        if (goals == null || goals.Length < 1)
        {
            Debug.LogError("No goals set !");
            return false;
        }

        flowFieldCells.Clear();

        GenerateDistanceField();
        GenerateVectorFields();

        return true;
    }

    private void GenerateDistanceField()
    {
        List<Vector2Int> searchingPosition = new List<Vector2Int>();

        foreach(var goal in goals)
        {
            searchingPosition.Add(goal);
            var cell = new FlowFieldCell(goal);
            flowFieldCells.Add(goal, cell);
        }


        while (searchingPosition.Count > 0)
        {
            int searchingPositionCount = searchingPosition.Count;
            for (int i = 0; i < searchingPositionCount; i++)
            {
                float weight = map.GetTileMovableWeight(searchingPosition[i]);
                if (weight == -1)
                    continue;

                var neighbours = map.GetAdjacentTiles(searchingPosition[i],true);
                foreach (var cur in neighbours)
                {
                    if (flowFieldCells.ContainsKey(cur) == true)
                        continue;

                    var searchingCell = flowFieldCells[searchingPosition[i]];

                    var cell = new FlowFieldCell(cur);
                    cell.distance = searchingCell.distance + (cur - searchingCell.position).magnitude;
                    flowFieldCells.Add(cur, cell);
                    searchingPosition.Add(cur);
                }
            }

            searchingPosition.RemoveRange(0, searchingPositionCount);
        }
    }

    private void GenerateVectorFields()
    {
        foreach (FlowFieldCell cur in flowFieldCells.Values)
        {
            var neighbours = map.GetAdjacentTiles(cur.position, true);

            FlowFieldCell nearCell = null;
            foreach (var neighbour in neighbours)
            {
                if (flowFieldCells.TryGetValue(neighbour, out FlowFieldCell neighbourCell) == true)
                {
                    if (nearCell == null || nearCell.distance < neighbourCell.distance)
                    {
                        nearCell = neighbourCell;
                    }
                }
            }

            Vector2 dir = nearCell.position - cur.position;
            dir.Normalize();
            cur.direction = dir;
        }
    }

    public Vector2 GetMovingDriection(Vector2Int currentPos)
    {
        if (flowFieldCells.TryGetValue(currentPos, out FlowFieldCell cell) == true)
            return cell.direction;

        return Vector2.zero;
    }
}
