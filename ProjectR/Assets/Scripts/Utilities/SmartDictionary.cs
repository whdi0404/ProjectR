namespace System.Collections.Generic
{
    public class SmartDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public new TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out TValue v) == true)
                    return v;

                return default;
            }
            set
            {
                if (ContainsKey(key) == true)
                    base[key] = value;
                else
                    Add(key, value);
            }
        }
    }
}