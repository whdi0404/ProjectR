using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPathFinderGraph<T> where T : IEquatable<T>
{
	public float GetTileMovableWeight( T pos );

	public IEnumerable<T> GetAdjacentTiles( T pos, bool includeDiagonal );
}

public class PathFinder<T> where T : IEquatable<T>
{
	private class PathFinderNode<t> : IComparable<PathFinderNode<t>>
	{
		public PathFinderNode<t> ParentNode { get; private set; }
		public t Pos { get; private set; }
		public float G { get; private set; }
		public float H { get; private set; }
		public float F { get => G + H; }

		public float W { get; private set; }

		public PathFinderNode( PathFinderNode<t> parentNode, t pos, float g, float h, float w )
		{
			ParentNode = parentNode;
			Pos = pos;
			G = g;
			H = h;
			W = w;
		}

		public int CompareTo( PathFinderNode<t> other )
		{
			return F.CompareTo( other.F );
		}

		public List<(t, float)> GetInversedNodeList( List<(t, float)> targetList = null )
		{
			if ( targetList == null || targetList.Count != 0 )
			{
				targetList = new List<(t, float)>();
			}

			PathFinderNode<t> current = this;

			while ( current != null )
			{
				targetList.Add( (current.Pos, current.W) );
				current = current.ParentNode;
			}

			targetList.Reverse();

			return targetList;
		}
	}

	private PriorityQueue<PathFinderNode<T>> open = new PriorityQueue<PathFinderNode<T>>();
	private Dictionary<T, PathFinderNode<T>> closed = new Dictionary<T, PathFinderNode<T>>();

	private Func<T, T, float> getDistanceLogic;

	public PathFinder( Func<T, T, float> getDistanceLogic )
	{
		this.getDistanceLogic = getDistanceLogic;
	}

	public List<(T, float)> FindPath( IPathFinderGraph<T> map, T startPos, T destPos, List<(T, float)> path = null )
	{
		PathFinderNode<T> startNode = new PathFinderNode<T>( null, startPos, 0, getDistanceLogic( startPos, destPos ), map.GetTileMovableWeight( startPos ) );
		open.Enqueue( startNode );
		closed.Add( startPos, startNode );

		PathFinderNode<T> destNode = null;

		while ( open.Count > 0 )
		{
			var currentNode = open.Dequeue();

			if ( destNode != null && currentNode.G >= destNode.G )
				continue;//이미 찾은 경로보다 더 많은 G를 이미 소모했으면

			if ( closed[ currentNode.Pos ] != currentNode )
				continue;//더 좋은 수가 생겼을때

			var adjucentTiles = map.GetAdjacentTiles( currentNode.Pos, true );

			foreach ( T nextPos in adjucentTiles )
			{
				float weight = map.GetTileMovableWeight( nextPos );
				if ( weight == -1 )
					continue;

				float toNextTileLength = getDistanceLogic( currentNode.Pos, nextPos );
				float nextPosG = currentNode.G + weight * toNextTileLength;

				if ( closed.TryGetValue( nextPos, out PathFinderNode<T> existNode ) == true )
				{
					//이걸 넣을지 뺄지
					if ( existNode.G > nextPosG )
					{
						PathFinderNode<T> nextNode = new PathFinderNode<T>( currentNode, nextPos, nextPosG, getDistanceLogic( nextPos, destPos ), weight );
						open.Enqueue( nextNode );
						closed[ nextPos ] = nextNode;

						if ( nextPos.Equals( destPos ) == true )
							destNode = nextNode;
					}
					else
						continue;
				}
				else
				{
					PathFinderNode<T> nextNode = new PathFinderNode<T>( currentNode, nextPos, nextPosG, getDistanceLogic( nextPos, destPos ), weight );
					open.Enqueue( nextNode );
					closed.Add( nextPos, nextNode );

					if ( nextPos.Equals( destPos ) == true )
						destNode = nextNode;
				}
			}
		}

		open.Clear();
		closed.Clear();

		return destNode.GetInversedNodeList( path );
	}
}