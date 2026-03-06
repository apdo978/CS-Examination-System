namespace ConsoleApp2
{
    public class Repository<T> where T : ICloneable, IComparable<T>
    {
        private T[] _items;
        private int _count;

        public Repository()
        {
            _items = new T[4];
            _count = 0;
        }

        public void Add(T item)
        {
            if (_count >= _items.Length)
            {
                var newArr = new T[_items.Length * 2];
                Array.Copy(_items, newArr, _count);
                _items = newArr;
            }
            _items[_count++] = item;
        }

        public void Remove(T item)
        {
            for (int i = 0; i < _count; i++)
            {
                if (_items[i].CompareTo(item) == 0)
                {
                    for (int j = i; j < _count - 1; j++)
                        _items[j] = _items[j + 1];
                    _items[--_count] = default!;
                    break;
                }
            }
        }

        public void Sort()
        {
            Array.Sort(_items, 0, _count);
        }

        public T[] GetAll()
        {
            var result = new T[_count];
            Array.Copy(_items, result, _count);
            return result;
        }
    }
}
