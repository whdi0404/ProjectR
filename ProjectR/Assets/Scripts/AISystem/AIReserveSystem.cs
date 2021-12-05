using BT;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ReserverBase
{
    public bool IsComplete { get; private set; }

    public object Source { get; private set; }
    public object Dest { get; private set; }

    public event Action onDestroyBeforeComplete;
    public event Action onDestroyAfterComplete;

    public void OnDestroy()
    {
        if (IsComplete == true)
            onDestroyAfterComplete?.Invoke();
        else
            onDestroyBeforeComplete?.Invoke();
    }

    public void Complete()
    {
        IsComplete = true;
    }

    public abstract void Destroy();
}

public abstract class ReserverBase<TSource, TDest> : ReserverBase
{
    public new TSource Source { get; private set; }
    public new TDest Dest { get; private set; }

    public ReserverBase(TSource Source, TDest Dest)
    {
        this.Source = Source;
        this.Dest = Dest;
    }
}

public abstract class ReserveSystemBase<TReserver, TSource, TDest> where TReserver : ReserverBase<TSource, TDest>
{
    private SmartDictionary<TSource, List<TReserver>> sourceDict = new SmartDictionary<TSource, List<TReserver>>();
    private SmartDictionary<TDest, List<TReserver>> destDict = new SmartDictionary<TDest, List<TReserver>>();

    public void AddReserver(TReserver reserver)
    {
        if (sourceDict.TryGetValue(reserver.Source, out var srclist) == false)
            sourceDict.Add(reserver.Source, srclist = new List<TReserver>());
        srclist.Add(reserver);

        if (destDict.TryGetValue(reserver.Dest, out var dstlist) == false)
            destDict.Add(reserver.Dest, dstlist = new List<TReserver>());
        dstlist.Add(reserver);
    }

    public void RemoveReserver(TReserver reserver)
    {
        if (sourceDict.TryGetValue(reserver.Source, out var srclist) == true)
        {
            srclist.Remove(reserver);
            if (srclist.Count == 0)
                sourceDict.Remove(reserver.Source);
        }

        if (destDict.TryGetValue(reserver.Dest, out var dstlist) == true)
        {
            dstlist.Remove(reserver);
            if (dstlist.Count == 0)
                destDict.Remove(reserver.Dest);
        }

        reserver.OnDestroy();
    }

    public List<TReserver> GetAllReserverFromSource(TSource source)
    {
        if (sourceDict.TryGetValue(source, out List<TReserver> reserverList) == true)
            return new List<TReserver>(reserverList);

        return new List<TReserver>(0);
    }

    public List<TReserver> GetAllReserverFromDest(TDest dest)
    {
        if (destDict.TryGetValue(dest, out List<TReserver> reserverList) == true)
            return new List<TReserver>(reserverList);

        return new List<TReserver>(0);
    }

    public TReserver GetReserver(TSource source, TDest dest)
    {
        return GetAllReserverFromSource(source).Find(reserver => reserver.Dest.Equals(dest));
    }

    public void RemoveAllReserverFromSource(TSource source)
    {
        if (sourceDict.TryGetValue(source, out var srclist) == true)
        {
            foreach (var reserver in srclist)
            {
                if (destDict.TryGetValue(reserver.Dest, out var destList) == true)
                {
                    destList.Remove(reserver);
                    if (destList.Count == 0)
                        destDict.Remove(reserver.Dest);

                    reserver.OnDestroy();
                }
            }
            sourceDict.Remove(source);
        }
    }

    public void RemoveAllReserverFromDest(TDest dest)
    {
        if (destDict.TryGetValue(dest, out var destlist) == true)
        {
            foreach (var reserver in destlist)
            {
                if (sourceDict.TryGetValue(reserver.Source, out var srclist) == true)
                {
                    srclist.Remove(reserver);
                    if (srclist.Count == 0)
                        sourceDict.Remove(reserver.Source);

                    reserver.OnDestroy();
                }
            }
            destDict.Remove(dest);
        }
    }
}
