using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZCanvas
{
    class Tuple<K, V>
    {
        public K Item1
        {
            get;
            set;
        }

        public V Item2
        {
            get;
            set;
        }

        public static Tuple<K, V>  Create(K k, V v)
        {
            return new Tuple<K, V>() { Item1 = k, Item2 = v};
        }
    }
}
