namespace System.Collections.Generic
{
    public class SmartDictionary<TKey, TValue> : Dictionary<TKey, TValue>
    {
        public SmartDictionary(){ }
        public SmartDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
        public SmartDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }

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