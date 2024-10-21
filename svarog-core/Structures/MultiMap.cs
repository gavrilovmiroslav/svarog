namespace svarog.Structures
{
    public class MultiMap<K, V> where K : notnull
    {
        private readonly Dictionary<K, List<V>> _dictionary = new();

        public void Add(K key, V value)
        {
            if (_dictionary.TryGetValue(key, out List<V>? list))
            {
                list?.Add(value);
            }
            else
            {
                list = new List<V>();
                list.Add(value);
                _dictionary[key] = list;
            }
        }

        public void Remove(K key, V value)
        {
            if (_dictionary.TryGetValue(key, out List<V>? list))
            {
                list?.Remove(value);
            }
        }

        public bool Contains(K key) => _dictionary.ContainsKey(key);

        public IEnumerable<K> Keys
        {
            get => _dictionary.Keys;
        }

        public List<V> this[K key]
        {
            get
            {
                if (_dictionary.TryGetValue(key, out List<V>? list))
                {
                    return list;
                }
                else
                {
                    list = new List<V>();
                    _dictionary[key] = list;
                    return list;
                }
            }
        }
    }
}