using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HierarchyDictionary<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
{
    protected Dictionary<TKey, HierarchyDictionary<TKey, TValue>> children;
    protected Dictionary<TKey,TValue> dictionary = new Dictionary<TKey, TValue>();

    public virtual void Add(TKey[] keys, TValue value)
    {
        Add(keys, value, 0);
    }

    protected virtual void Add(TKey[] keys, TValue value, int index)
    {
        if (keys.Length == index + 1)
        {
            dictionary.Add(keys[index], value);
        }
        else
        {
            if (children == null)
                children = new Dictionary<TKey, HierarchyDictionary<TKey, TValue>>();

            if (children.TryGetValue(keys[index], out var child) == false)
            {
                child = new HierarchyDictionary<TKey, TValue>();
                children.Add(keys[index], child);
            }

            child.Add(keys, value, ++index);
        }
    }

    public virtual bool ContainsKey(TKey[] keys)
    {
        return ContainsKey(keys, 0);
    }

    protected virtual bool ContainsKey(TKey[] keys, int index)
    {
        if (keys.Length == index + 1)
        {
            return dictionary.ContainsKey(keys[index]);
        }
        else
        {
            if (children == null)
                return false;

            if (children.TryGetValue(keys[index], out var child) == true)
                return child.ContainsKey(keys, index);

            return false;
        }
    }

    public bool Remove(TKey[] keys)
    {
        return Remove(keys, 0);
    }

    protected bool Remove(TKey[] keys, int index)
    {
        if (keys.Length == index + 1)
        {
            return dictionary.Remove(keys[index]);
        }
        else
        {
            if (children == null)
                return false;

            if (children.TryGetValue(keys[index], out var child) == true)
                return child.Remove(keys, index);

            return false;
        }
    }

    public bool TryGetValue(TKey[] keys, out TValue value)
    {
        return TryGetValue(keys, out value, 0);
    }

    protected bool TryGetValue(TKey[] keys, out TValue value, int index)
    {

        if (keys.Length == index + 1)
        {
            return dictionary.TryGetValue(keys[index], out value);
        }
        else
        {
            value = default(TValue);
            if (children == null)
                return false;

            if (children.TryGetValue(keys[index], out var child) == true)
                return child.TryGetValue(keys, out value, index);

            return false;
        }
    }

    public bool TryGetValues(TKey[] keys, out List<TValue> values)
    {
        return TryGetValues(keys, out values, 0);
    }

    protected bool TryGetValues(TKey[] keys, out List<TValue> values, int index)
    {
        values = new List<TValue>();

        if (keys.Length == index + 1)
        {
            if (dictionary.TryGetValue(keys[index], out var value) == true)
            {
                values.Add(value);
            }

            Stack<HierarchyDictionary<TKey, TValue>> stack = new Stack<HierarchyDictionary<TKey, TValue>>();

            if (children != null)
            {
                foreach (var child in children.Values)
                {
                    stack.Push(child);
                }
            }
            

            while (stack.Count > 0)
            {
                var dict = stack.Pop();

                values.AddRange(dict.dictionary.Values);

                if (dict.children != null)
                {
                    foreach (var child in dict.children.Values)
                    {
                        stack.Push(child);
                    }
                }
            }

            return values.Count > 0;
        }
        else
        {
            return TryGetValues(keys, out values, ++index);
        }
    }

    public bool TryGetLeafValue(TKey key, out TValue value)
    {
        return TryGetLeafValue(key, out value, 0);
    }

    protected bool TryGetLeafValue(TKey key, out TValue value, int index)
    {
        Stack<HierarchyDictionary<TKey, TValue>> stack = new Stack<HierarchyDictionary<TKey, TValue>>();

        stack.Push(this);

        while (stack.Count > 0)
        {
            var dict = stack.Pop();

            if (dict.dictionary.TryGetValue(key, out value) == true)
                return true;

            if (dict.children != null)
            {
                foreach (var child in dict.children.Values)
                    stack.Push(child);
            }
        }

        value = default(TValue);
        return false;
    }

    public List<TValue> GetAllValues()
    {
        List<TValue> values = new List<TValue>();

        Stack<HierarchyDictionary<TKey, TValue>> stack = new Stack<HierarchyDictionary<TKey, TValue>>();

        stack.Push(this);

        while (stack.Count > 0)
        {
            var dict = stack.Pop();

            values.AddRange(dict.dictionary.Values);
            if (dict.children != null)
            {
                foreach (var child in dict.children.Values)
                {
                    stack.Push(child);
                }
            }
        }

        return values;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new Enumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return new Enumerator(this);
    }

    public class Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        List<KeyValuePair<TKey, TValue>> values = new List<KeyValuePair<TKey, TValue>>();
        int position = -1;

        public Enumerator(HierarchyDictionary<TKey, TValue> dict)
        {
            Stack<HierarchyDictionary<TKey, TValue>> stack = new Stack<HierarchyDictionary<TKey, TValue>>();
            stack.Push(dict);

            while (stack.Count > 0)
            {
                var stackDict = stack.Pop();

                values.AddRange(stackDict.dictionary);

                if (stackDict.children != null)
                {
                    foreach (var child in stackDict.children.Values)
                    {
                        stack.Push(child);
                    }
                }
            }
        }

        public KeyValuePair<TKey, TValue> Current
        {
            get
            {
                try
                {
                    return values[position];
                }
                catch (IndexOutOfRangeException)
                {
                    throw new InvalidOperationException();
                }
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            position++;
            return (position < values.Count);
        }

        public void Reset()
        {
            position = -1;
        }
    }
}