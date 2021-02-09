using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PriorityQueue<T> where T : IComparable<T>
{
	private List<T> container;
	private bool isDescending;

	public int Count { get => container.Count; }
	private int CompSign { get => isDescending ? 1 : -1; }

	public PriorityQueue( bool isDescending = false )
	{
		container = new List<T>();
		this.isDescending = isDescending;
	}

	public void Enqueue(T newData)
	{
		int currentIndex = container.Count;
		container.Add( newData );
		
		if ( currentIndex == 0 )
			return;

		do
		{
			int parentIndex = GetParentIndex( currentIndex );

			if ( newData.CompareTo( container[ parentIndex ] ) == CompSign )
			{
				T parentData = container[ parentIndex ];
				container[ parentIndex ] = newData;
				container[ currentIndex ] = parentData;
				currentIndex = parentIndex;
			}
			else
				break;

		}
		while ( true );
	}

	public T Dequeue()
	{
		T retVal = container[ 0 ];

		T sortData = container[ 0 ] = container[ container.Count - 1 ];
		container.RemoveAt( container.Count - 1 );

		int currentIndex = 0;
		do
		{
			int rightChildIndex = GetRightChildIndex( currentIndex );
			int leftChildIndex = rightChildIndex - 1;

			if ( leftChildIndex >= container.Count )
				break;

			int leftComp = sortData.CompareTo( container[ leftChildIndex ] );
			int rightComp = rightChildIndex < container.Count ? sortData.CompareTo( container[ rightChildIndex ] ) : 0;

			int sortIndex;

			if ( leftComp == -CompSign && rightComp == -CompSign )
			{
				if ( container[ leftChildIndex ].CompareTo( container[ rightChildIndex ] ) == CompSign )
					sortIndex = leftChildIndex;
				else
					sortIndex = rightChildIndex;
			}
			else if ( leftComp == -CompSign )
				sortIndex = leftChildIndex;
			else if ( rightComp == -CompSign )
				sortIndex = rightChildIndex;
			else
				break;

			T temp = container[ sortIndex ];
			container[ sortIndex ] = sortData;
			container[ currentIndex ] = temp;
			currentIndex = sortIndex;
		}
		while ( true );

		return retVal;
	}

	private static int GetParentIndex( int index )
	{
		return ( index - 1 ) / 2;
	}

	private static int GetRightChildIndex( int index )
	{
		return ( index + 1 ) * 2;
	}
}

public class PathFinder
{
	private Map map;
	private PriorityQueue<int> heap;
	
	public void Init( Map map )
	{
		this.map = map;
		heap = new PriorityQueue<int>();
	}

}
