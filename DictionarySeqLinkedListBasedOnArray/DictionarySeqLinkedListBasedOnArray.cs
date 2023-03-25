using IDictionarySeq.DictionarySeqLinkedListBasedOnArray.CustomLinkedListBasedOnArray;
using IDictionarySeq.DictionarySeqLinkedListBasedOnArray.LinkedListOnStruct;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace IDictionarySeq
{
    //Более простой и эффективный вариант

    /// <summary>
    /// Кастомный словарь с сохранением порядка вставки элементов при перечислении значений]
    /// Более трудоемкий вариант, приходится переопределять практически все методы интерфейса
    /// </summary>
    /// <typeparam name="T">Тип ключа</typeparam>
    /// <typeparam name="U">Тип значения</typeparam>
    internal class DictionarySeqLinkedListBasedOnArray<T, U> : IDictionary<T, U>
    {
        /// <summary>
        /// Связанный список для хранения порядка элементор
        /// </summary>
        private CustomLinkedListBasedOnArray<U> _list = new CustomLinkedListBasedOnArray<U>();

        /// <summary>
        /// Словарь с соответствием Ключ-индекс. Индекс растет с порядковым номером вставки элемента
        /// Использование словаря гарантирует ту же самую алгоритмическую сложность при всех операциях :)
        /// </summary>
        private IDictionary<T, int> _dictionary_key_to_index = new Dictionary<T, int>();

        /// <summary>
        /// Примитив синхронизации
        /// </summary>
        private object _lock = new object();

        /// <summary>
        /// Да, нельзя использовать OrderedQueue или аналоги - log(N) при вставке\удалении это другая алгоритмическая сложность.
        /// Остается только сортировка при вызове перечисления элементов
        /// </summary>
        public ICollection<U> Values => (ICollection<U>)_list;

        public void Add(T key, U value)
        {
            lock (_lock)
            {
                var node = _list.AddLast(value);
                _dictionary_key_to_index.Add(key, node);
            }
        }

        public void Add(KeyValuePair<T, U> item)
        {
            lock (_lock)
            {
                var node = _list.AddLast(item.Value);
                _dictionary_key_to_index.Add(new KeyValuePair<T, int>(item.Key, node));
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _dictionary_key_to_index.Clear();
                _list.Clear();
            }
        }

        public bool Remove(T key)
        {
            lock (_lock)
            {
                int node;

                if (_dictionary_key_to_index.TryGetValue(key, out node))
                {
                    _list.Remove(_list[node]);

                    _dictionary_key_to_index.Remove(key);

                    return true;
                }
            }

            return false;
        }

        public bool Remove(KeyValuePair<T, U> item)
        {
            lock (_lock)
            {
                int node;

                if (_dictionary_key_to_index.TryGetValue(item.Key, out node))
                {
                    _list.Remove(_list[node]);

                    _dictionary_key_to_index.Remove(item.Key);

                    return true;
                }
            }

            return false; ;
        }

        #region Неинтересные переопределения
        public U this[T key] 
        { 
            get
            {
                var index = _dictionary_key_to_index[key];
                return _list[index].Value;
            }
            set   
            {
                var oldNodeIndex = _dictionary_key_to_index[key];

                var oldNode = _list[oldNodeIndex];

                var newNode = new CustomNode<U>();

                newNode.Next = oldNode.Next;
                newNode.Previos = oldNode.Previos;
                newNode.Value = value;

                _list[oldNodeIndex] = newNode;

                _dictionary_key_to_index[key] = oldNodeIndex;
            }
        }

        public ICollection<T> Keys => _dictionary_key_to_index.Keys;

        public int Count => _dictionary_key_to_index.Count;

        public bool IsReadOnly => _dictionary_key_to_index.IsReadOnly;


        public bool Contains(KeyValuePair<T, U> item)
        {
            return _dictionary_key_to_index.ContainsKey(item.Key);
        }

        public bool ContainsKey(T key)
        {
            return _dictionary_key_to_index.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<T, U>[] array, int arrayIndex)
        {
            _dictionary_key_to_index.Select(item => new KeyValuePair<T, U>(item.Key, _list[item.Value].Value)).ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
        {
            return _dictionary_key_to_index.Select(item => new KeyValuePair<T, U>(item.Key, _list[item.Value].Value)).GetEnumerator();
        }


        public bool TryGetValue(T key, [MaybeNullWhen(false)] out U value)
        {
            int val;
            if (_dictionary_key_to_index.TryGetValue(key, out val))
            {
                value = _list[val].Value;
                return true;
            }

            value = default(U);

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary_key_to_index.Select(item => new KeyValuePair<T, U>(item.Key, _list[item.Value].Value)).GetEnumerator();
        }
        #endregion
    }
}

