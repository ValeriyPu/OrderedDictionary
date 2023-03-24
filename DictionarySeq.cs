using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IDictionarySeq
{
    internal class DictionarySeq<T, U> : IDictionary<T, U>
    {
        /// <summary>
        /// Словарь с элементами
        /// </summary>
        private IDictionary<T,U> _dictionary_val = new Dictionary<T,U>();

        /// <summary>
        /// Индекс элемента (увеличивается при вставке)
        /// </summary>
        private int index = 0;

        /// <summary>
        /// Словарь с соответствием Ключ-индекс. Индекс растет с порядковым номером вставки элемента
        /// Использование словаря гарантирует ту же самую алгоритмическую сложность при всех операциях :)
        /// </summary>
        private IDictionary<T,int> _dictionary_key_to_index = new Dictionary<T,int>();

        /// <summary>
        /// Примитив синхронизации
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Да, нельзя использовать OrderedQueue или аналоги - log(N) при вставке\удалении это другая алгоритмическая сложность.
        /// Остается только сортировка при вызове перечисления элементов
        /// </summary>
        public ICollection<U> Values => (ICollection<U>)_dictionary_key_to_index.OrderBy(item => item.Value).Select(item => _dictionary_val[item.Key]).ToList();

        public void Add(T key, U value)
        {
            lock (_lock)
            {
                _dictionary_key_to_index.Add(key, index);
                _dictionary_val.Add(key, value);
                index++;
            }
        }

        public void Add(KeyValuePair<T, U> item)
        {
            lock (_lock)
            {
                _dictionary_key_to_index.Add(new KeyValuePair<T, int>(item.Key,index));
                _dictionary_val.Add(item);
                index++;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _dictionary_key_to_index.Clear();
                _dictionary_val.Clear();
                index = 0;
            }
        }

        public bool Remove(T key)
        {
            var res = false;

            lock (_lock)
            {
                res = _dictionary_val.Remove(key);
                if (res)
                    _dictionary_key_to_index.Remove(key);
            }

            return res;
        }

        public bool Remove(KeyValuePair<T, U> item)
        {
            var res = false;

            lock (_lock)
            {
                res = _dictionary_val.Remove(item);
                if (res)
                    _dictionary_key_to_index.Remove(item.Key);
            }
            return res;
        }

        #region Неинтересные переопределения
        public U this[T key] { get => _dictionary_val[key]; set => _dictionary_val[key] = value; }

        public ICollection<T> Keys => _dictionary_val.Keys;

        public int Count => _dictionary_val.Count;

        public bool IsReadOnly => _dictionary_val.IsReadOnly;


        public bool Contains(KeyValuePair<T, U> item)
        {
            return _dictionary_val.Contains(item);
        }

        public bool ContainsKey(T key)
        {
            return _dictionary_val.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex)
        {
            _dictionary_val.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
        {
            return _dictionary_val.GetEnumerator();
        }


        public bool TryGetValue(T key, [MaybeNullWhen(false)] out U value)
        {
            return _dictionary_val.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary_val.GetEnumerator();
        }
        #endregion
    }
}
