using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static WorldMap;

public interface IRegionListener
{
	public void OnRegionChange( List<LocalRegion> removedLocalRegions, List<LocalRegion> newLocalRegions );
}

public class LocalRegion : IEquatable<LocalRegion>
{
    private HashSet<Vector2Int> tiles;

    private HashSet<Vector2Int> outlineTiles;
    private HashSet<Vector2Int> adjacentTiles;

    private WorldMap worldMap;

    public HashSet<LocalRegion> AdjacentRegion { get; private set; } = new HashSet<LocalRegion>();

    //벽 등의 이동 불가 지역인지
    public bool IsClosedRegion { get; private set; }

    public Vector2Int GroupIndex { get; private set; }

    public BoundsInt Bounds { get; private set; }

    public bool CheckLeft { get; private set; }
    public bool CheckRight { get; private set; }
    public bool CheckUp { get; private set; }
    public bool CheckDown { get; private set; }

    public LocalRegion(WorldMap worldMap, Vector2Int groupIndex, List<Vector2Int> tiles, bool isClosedRegion)
    {
        this.worldMap = worldMap;
        //바운드박스 만들어서 선체크 후, 겹치면 인접체크 하자.
        //인접한 타일들 따로 모아서 그것들끼리만 체크하자.

        this.IsClosedRegion = isClosedRegion;
        GroupIndex = groupIndex;
        this.tiles = new HashSet<Vector2Int>(tiles);

        CalculateBounds();
        CalculateOutline();
    }

    private void CalculateBounds()
    {
        Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);
        Vector2Int max = new Vector2Int(int.MinValue, int.MinValue);

        foreach (var tile in tiles)
        {
            min = Vector2Int.Min(min, tile);
            max = Vector2Int.Max(max, tile);
        }
        max += Vector2Int.one;

        BoundsInt bounds = new BoundsInt();
        bounds.SetMinMax(new Vector3Int(min.x, min.y, 0), new Vector3Int(max.x, max.y, 0));
        Bounds = bounds;

        CheckLeft = Bounds.xMin == GroupIndex.x * worldMap.TileGroupSize.x;
        CheckRight = Bounds.xMax == (GroupIndex.x + 1) * worldMap.TileGroupSize.x;
        CheckDown = Bounds.yMin == GroupIndex.y * worldMap.TileGroupSize.y;
        CheckUp = Bounds.yMax == (GroupIndex.y + 1) * worldMap.TileGroupSize.y;
    }

    private void CalculateOutline()
    {
        outlineTiles = new HashSet<Vector2Int>();
        adjacentTiles = new HashSet<Vector2Int>();

        foreach (var tile in tiles)
        {
            if (tiles.Contains(tile + new Vector2Int(-1, 0)) == false)
            {
                outlineTiles.Add(tile);
                adjacentTiles.Add(tile + new Vector2Int(-1, 0));
            }
            if (tiles.Contains(tile + new Vector2Int(1, 0)) == false)
            {
                outlineTiles.Add(tile);
                adjacentTiles.Add(tile + new Vector2Int(1, 0));
            }
            if (tiles.Contains(tile + new Vector2Int(0, -1)) == false)
            {
                outlineTiles.Add(tile);
                adjacentTiles.Add(tile + new Vector2Int(0, -1));
            }
            if (tiles.Contains(tile + new Vector2Int(0, 1)) == false)
            {
                outlineTiles.Add(tile);
                adjacentTiles.Add(tile + new Vector2Int(0, 1));
            }
        }
    }

    public bool IsAdjacent(LocalRegion localRegion)
    {
        if (Bounds.Intersects(localRegion.Bounds) == true)
        {
            foreach (var checkTile in adjacentTiles)
            {
                if (localRegion.outlineTiles.Contains(checkTile) == true)
                    return true;
            }
        }   

        return false;
    }

    public bool IsIn(Vector2Int pos)
    {
        return tiles.Contains(pos);
    }

    public bool Equals(LocalRegion other)
    {
        return this == other;
    }

    public void ReleaseAllLink()
    {
        foreach (var region in AdjacentRegion)
            region.AdjacentRegion.Remove(this);

        AdjacentRegion.Clear();
    }

    public static void Link(LocalRegion a, LocalRegion b)
    {
        a.AdjacentRegion.Add(b);
        b.AdjacentRegion.Add(a);
    }

    public void DrawOnGUI(Color color, bool drawAdjacent)
    {
        foreach (var tile in outlineTiles)
        {
            Vector2 start = CameraManager.Instance.MainCamera.WorldToScreenPoint(new Vector2(tile.x, tile.y));
            start.y = Screen.height - start.y;

            Vector2 end = CameraManager.Instance.MainCamera.WorldToScreenPoint(new Vector2(tile.x, tile.y) + new Vector2(1, 1));
            end.y = Screen.height - end.y;

            GUIExt.DrawRect(Rect.MinMaxRect(start.x, end.y, end.x, start.y), color);
        }

        Vector2 center = CameraManager.Instance.MainCamera.WorldToScreenPoint(Bounds.center);
        center.y = Screen.height - center.y;

        GUI.Label(new Rect(center, new Vector2(100, 25)), $"{GroupIndex}");

        if (drawAdjacent)
            foreach (var adjacentRegion in AdjacentRegion)
                adjacentRegion.DrawOnGUI(Color.yellow, false);
    }
}

public class RegionSystem
{
    private WorldMap worldMap;
    //TileGroupIndex/regions
    private SmartDictionary<Vector2Int, List<LocalRegion>> regions = new SmartDictionary<Vector2Int, List<LocalRegion>>();

    private SmartDictionary<LocalRegion, SmartDictionary<LocalRegion, (LocalRegion, int)>> allNodesDijkstraMap = new SmartDictionary<LocalRegion, SmartDictionary<LocalRegion, (LocalRegion, int)>>();

    private event Action<List<LocalRegion>, List<LocalRegion>> onRegionChangeEvent;
    public void Initialize(WorldMap worldMap)
    {
        this.worldMap = worldMap;
        foreach (Vector2Int groupIndex in worldMap.ExistGroupList)
            CalculateLocalRegion(groupIndex, false);

        RefreshDijkstraMap();
    }

    //길찾기 경로 중 재생성하는 Region이 포함된 타일이 있으면 Region 재생성시 이동중인 애들한테 알려줘야함.
    public void CalculateLocalRegion(Vector2Int groupIndex, bool refreshDijkstraMap = true)
    {
        if (regions.TryGetValue(groupIndex, out List<LocalRegion> oldRegionList) == true)
        {
            oldRegionList.ForEach((region) => region.ReleaseAllLink());
            regions.Remove(groupIndex);
        }

        if (worldMap.TryGetTileFragments(groupIndex, out TileFragmentData tileFragmentData) == false)
            return;

        HashSet<Vector2Int> movableTiles = new HashSet<Vector2Int>();
        List<Vector2Int> cantMoveTiles = new List<Vector2Int>();

        for (int x = 0; x < worldMap.TileGroupSize.x; ++x)
            for (int y = 0; y < worldMap.TileGroupSize.y; ++y)
            {
                var tileDesc = tileFragmentData.fragmentData[x + y * worldMap.TileGroupSize.x];
                if (tileDesc.MoveWeight == 0)
                    cantMoveTiles.Add(new Vector2Int(tileFragmentData.bounds.xMin + x, tileFragmentData.bounds.yMin + y));
                else
                    movableTiles.Add(new Vector2Int(tileFragmentData.bounds.xMin + x, tileFragmentData.bounds.yMin + y));
            }

        List<LocalRegion> regionList = new List<LocalRegion>();
        //closedRegion 추가
        regionList.Add(new LocalRegion(worldMap, groupIndex, cantMoveTiles, true));
        while (movableTiles.Count > 0)
        {
            Stack<Vector2Int> searchStack = new Stack<Vector2Int>();
            List<Vector2Int> newRegionTiles = new List<Vector2Int>();

            Vector2Int firstTile = movableTiles.First();

            searchStack.Push(firstTile);
            newRegionTiles.Add(firstTile);
            movableTiles.Remove(firstTile);

            while (searchStack.Count > 0)
            {
                Vector2Int searchTile = searchStack.Pop();

                Vector2Int left = searchTile + new Vector2Int(-1, 0);
                if (movableTiles.Contains(left) == true)
                {
                    searchStack.Push(left);
                    newRegionTiles.Add(left);
                    movableTiles.Remove(left);
                }

                Vector2Int right = searchTile + new Vector2Int(1, 0);
                if (movableTiles.Contains(right) == true)
                {
                    searchStack.Push(right);
                    newRegionTiles.Add(right);
                    movableTiles.Remove(right);
                }

                Vector2Int down = searchTile + new Vector2Int(0, -1);
                if (movableTiles.Contains(down) == true)
                {
                    searchStack.Push(down);
                    newRegionTiles.Add(down);
                    movableTiles.Remove(down);
                }

                Vector2Int up = searchTile + new Vector2Int(0, 1);
                if (movableTiles.Contains(up) == true)
                {
                    searchStack.Push(up);
                    newRegionTiles.Add(up);
                    movableTiles.Remove(up);
                }
            }
            LocalRegion newRegion = new LocalRegion(worldMap, groupIndex, newRegionTiles, false);
            regionList.Add(newRegion);
            CalculateAdjecentRegion(newRegion);
        }
        regions.Add(groupIndex, regionList);

        onRegionChangeEvent?.Invoke(oldRegionList, regionList);

        if (refreshDijkstraMap == true)
            RefreshDijkstraMap();
    }

    private void CalculateAdjecentRegion(LocalRegion targetRegion)
    {
        if (targetRegion.IsClosedRegion == true)
            return;

        List<LocalRegion> regionList;
        if (regions.TryGetValue(targetRegion.GroupIndex, out regionList) == true)
        {
            foreach (LocalRegion region in regionList)
            {
                if (region == targetRegion || region.IsClosedRegion == true)
                    continue;

                if (targetRegion.Bounds.Intersects(region.Bounds) == true)
                    LocalRegion.Link(targetRegion, region);
            }
        }

        if (targetRegion.CheckLeft == true && regions.TryGetValue(targetRegion.GroupIndex + new Vector2Int(-1, 0), out regionList) == true)
        {
            foreach (LocalRegion region in regionList)
                if (targetRegion.IsAdjacent(region) == true && region.IsClosedRegion == false)
                    LocalRegion.Link(targetRegion, region);
        }
        if (targetRegion.CheckRight == true && regions.TryGetValue(targetRegion.GroupIndex + new Vector2Int(1, 0), out regionList) == true)
        {
            foreach (LocalRegion region in regionList)
                if (targetRegion.IsAdjacent(region) == true && region.IsClosedRegion == false)
                    LocalRegion.Link(targetRegion, region);
        }
        if (targetRegion.CheckDown == true && regions.TryGetValue(targetRegion.GroupIndex + new Vector2Int(0, -1), out regionList) == true)
        {
            foreach (LocalRegion region in regionList)
                if (targetRegion.IsAdjacent(region) == true && region.IsClosedRegion == false)
                    LocalRegion.Link(targetRegion, region);
        }
        if (targetRegion.CheckUp == true && regions.TryGetValue(targetRegion.GroupIndex + new Vector2Int(0, 1), out regionList) == true)
        {
            foreach (LocalRegion region in regionList)
                if (targetRegion.IsAdjacent(region) == true && region.IsClosedRegion == false)
                    LocalRegion.Link(targetRegion, region);
        }
    }

    public bool GetRegionFromTilePos(Vector2Int tile, out LocalRegion containRegion)
    {
        containRegion = null;
        Vector2Int groupIndex = worldMap.TilePosToGroupIndex(tile);
        if (regions.TryGetValue(groupIndex, out List<LocalRegion> regionList) == true)
        {
            foreach (LocalRegion r in regionList)
            {
                if (r.IsIn(tile) == true)
                {
                    containRegion = r;
                    return true;
                }
            }
        }

        return false;
    }

    public bool IsReachable(Vector2Int start, Vector2Int dest)
    {
        if (GetRegionFromTilePos(start, out var startRegion) == false)
            return false;
        if (GetRegionFromTilePos(dest, out var destRegion) == false)
            return false;

        return IsReachable(startRegion, destRegion);
    }

    public bool IsReachable(LocalRegion start, LocalRegion dest)
    {
        if (allNodesDijkstraMap.TryGetValue(start, out var dijkstraMap) == false)
            return false;

        return dijkstraMap.TryGetValue(dest, out var cost);
    }

    public List<LocalRegion> GetAllReachableRegions(LocalRegion region)
    {
        if (this.allNodesDijkstraMap.TryGetValue(region, out var regions) == true)
        {
            var reachableList = regions.Keys.ToList();
            reachableList.Add(region);

            return reachableList;
        }

        return new List<LocalRegion>() { region };
    }

    public float GetTileMovableWeight(LocalRegion pos)
    {
        return 1f;
    }

    public IEnumerable<(LocalRegion, float)> GetMovableAdjacentTiles(LocalRegion pos, bool includeDiagonal)
    {
        foreach (var region in pos.AdjacentRegion)
            yield return (region, 1);
    }

    public void AddListener(IRegionListener listener)
    {
        onRegionChangeEvent += listener.OnRegionChange;
    }
    public void RemoveListener(IRegionListener listener)
    {
        onRegionChangeEvent -= listener.OnRegionChange;
    }

    //다익스트라 관련
    public class DijkstraNode : IComparable<DijkstraNode>
    {
        public LocalRegion region;
        public int cost;

        public DijkstraNode(LocalRegion region, int cost)
        {
            this.region = region;
            this.cost = cost;
        }

        public int CompareTo(DijkstraNode other)
        {
            return cost.CompareTo(other.cost);
        }
    }

    public void RefreshDijkstraMap()
    {
        allNodesDijkstraMap = new SmartDictionary<LocalRegion, SmartDictionary<LocalRegion, (LocalRegion, int)>>();
        foreach (var regionList in regions.Values)
        {
            foreach (var region in regionList)
                allNodesDijkstraMap[region] = Dijkstra(region);
        }
    }

    //Key: 목표위치 Value: (이전위치,비용)
    //길찾기에 활용하기 위해 이전위치를 저장함. 목표위치까지의 경로를 알고싶으면, 목표위치의 이전위치를 순차적으로 찾아가면 됨.
    SmartDictionary<LocalRegion, (LocalRegion, int)> Dijkstra(LocalRegion startNode)
    {
        SmartDictionary<LocalRegion, (LocalRegion, int)> djikstraDict = new SmartDictionary<LocalRegion, (LocalRegion, int)>();

        PriorityQueue<DijkstraNode> pq = new PriorityQueue<DijkstraNode>();
        pq.Enqueue(new DijkstraNode(startNode, 0));
        djikstraDict[startNode] = (null, 0);

        while (pq.Count > 0)
        {
            DijkstraNode currentNode = pq.Dequeue();

            foreach (var nextRegion in currentNode.region.AdjacentRegion)
            {
                int prevCost = djikstraDict[nextRegion].Item2;
                int nextCost = currentNode.cost + 1;

                if (prevCost == 0 || prevCost > nextCost)
                {
                    djikstraDict[nextRegion] = (currentNode.region, nextCost);
                    pq.Enqueue(new DijkstraNode(nextRegion, nextCost));
                }
            }
        }

        return djikstraDict;
    }
}