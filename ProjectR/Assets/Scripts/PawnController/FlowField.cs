using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowFieldCell
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

public class FlowField
{
    Map map;
    private Vector2Int[] goals;
    public FlowField(Map map, Vector2Int[] goals)
    {
        this.map = map;
        this.goals = goals;
    }
    /// <summary>
    /// Wavefront algorithm to create a distance field.
    /// </summary>
    public void GenerateDistanceField()
    {
        if (goals == null || goals.Length < 1)
        {
            Debug.LogError("No goals set !");
            return;
        }

        var marked = new List<FlowFieldCell>();

        foreach(var goal in goals)
        {
            marked.Add(new FlowFieldCell(goal));
        }


        while (marked.Count < map.Length)
        {
            for (int i = 0; i < marked.Count; i++)
            {
                if (marked[i].unpassable)
                    continue;

                var neighbours = grid.GetMooreNeighbours(marked[i]);
                for (int j = 0; j < 8; j++)
                {
                    var cur = neighbours[j];
                    if (cur == null || marked.Contains(cur))
                        continue;

                    cur.distance = marked[i].distance;
                    cur.distance += (cur.position - marked[i].position).magnitude;

                    marked.Add(cur);
                }
            }
        }
    }


    public void GenerateVectorFields()
    {
        for (int i = 0; i < grid.cells.Length; i++)
        {
            var cur = grid.cells[i];
            var neighbours = grid.GetNeumannNeighbours(cur);

            float left, right, up, down;
            left = right = up = down = cur.distance;

            if (neighbours[0] != null && !neighbours[0].unpassable) up = neighbours[0].distance;
            if (neighbours[1] != null && !neighbours[1].unpassable) right = neighbours[1].distance;
            if (neighbours[2] != null && !neighbours[2].unpassable) down = neighbours[2].distance;
            if (neighbours[3] != null && !neighbours[3].unpassable) left = neighbours[3].distance;


            float x = left - right;
            float y = down - up;

            cur.direction = new Vector2(x, y);
            cur.direction.Normalize();
        }
    }
}
