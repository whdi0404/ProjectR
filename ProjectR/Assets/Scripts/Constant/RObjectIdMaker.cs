using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���� �з��������� ã���ִ°�?
public abstract class RObjectIndexer<TKey>
{
    public abstract TKey MakeKey(RObject obj);
}