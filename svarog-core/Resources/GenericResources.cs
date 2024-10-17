using System.Collections.Generic;

namespace svarog
{
    public partial class Resources
    {
        private Dictionary<string, object> bag = [];

        public T Bag<T>(string name, T value) where T : notnull
        {
            bag[name] = value;
            return value;
        }

        public void RemoveFromBag(string name)
        {
            bag.Remove(name);
        }

        public T? GetFromBag<T>(string name)
        {
            if (bag.TryGetValue(name, out var value))
            {
                if (value is T t)
                {
                    return t;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                return default;
            }
        }
    }
}