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

    private WorldMap map;
    private Vector2Int goal;
    private Dictionary<Vector2Int, FlowFieldCell> flowFieldCells = new Dictionary<Vector2Int, FlowFieldCell>();

    public FlowField(WorldMap map)
    {
        this.map = map;
    }

    public bool Calculate(Vector2Int startPos, Vector2Int goal)
    {
        this.goal = goal;

        flowFieldCells.Clear();

        GenerateDistanceField();

        if (flowFieldCells.ContainsKey(startPos) == false)
            return false;
        GenerateVectorFields();

        return true;
    }

    private void GenerateDistanceField()
    {
        List<Vector2Int> searchingPosition = new List<Vector2Int>();

        searchingPosition.Add(goal);
        var goalCell = new FlowFieldCell(goal);
        flowFieldCells.Add(goal, goalCell);

        while (searchingPosition.Count > 0)
        {
            int searchingPositionCount = searchingPosition.Count;
            for (int i = 0; i < searchingPositionCount; i++)
            {
                AtlasInfoDescriptor tmp;
                if (map.TryGetTile(searchingPosition[i], out tmp) == false || tmp.MoveWeight == 0)
                    continue;

                var neighbours = map.GetAdjacentTiles(searchingPosition[i], true);
                foreach (var cur in neighbours)
                {
                    if (flowFieldCells.ContainsKey(cur) == true)
                        continue;

                    if (map.TryGetTile(cur, out tmp) == false || tmp.MoveWeight == 0)
                        continue;

                    var searchingCell = flowFieldCells[searchingPosition[i]];

                    var cell = new FlowFieldCell(cur);
                    cell.distance = searchingCell.distance + (cur - searchingCell.position).magnitude * tmp.MoveWeight;
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
