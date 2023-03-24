using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace IDictionarySeq
{
    //Более простой и эффективный вариант

    /// <summary>
    /// Кастомный словарь с сохранением порядка вставки элементов при перечислении значений]
    /// Более трудоемкий вариант, приходится переопределять практически все методы интерфейса
    /// </summary>
    /// <typeparam name="T">Тип ключа</typeparam>
    /// <typeparam name="U">Тип значения</typeparam>
    internal class DictionarySeqLinkedList<T, U> : IDictionary<T, U>
    {
        /// <summary>
        /// Связанный список для хранения порядка элементор
        /// </summary>
        private CustomLinkedList<U> _list = new CustomLinkedList<U>();

        /// <summary>
        /// Словарь с соответствием Ключ-индекс. Индекс растет с порядковым номером вставки элемента
        /// Использование словаря гарантирует ту же самую алгоритмическую сложность при всех операциях :)
        /// </summary>
        private IDictionary<T, CustomLinkedListNode<U>> _dictionary_key_to_index = new Dictionary<T, CustomLinkedListNode<U>>();

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
                _dictionary_key_to_index.Add(new KeyValuePair<T, CustomLinkedListNode<U>>(item.Key, node));
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
                CustomLinkedListNode<U> node;
                    
                if (_dictionary_key_to_index.TryGetValue(key, out node))
                {
                    _list.Remove(node);

                    _dictionary_key_to_index.Remove(key);

                    return true;
                }
            }

            return false; ;
        }

        public bool Remove(KeyValuePair<T, U> item)
        {
            lock (_lock)
            {
                CustomLinkedListNode<U> node;

                if (_dictionary_key_to_index.TryGetValue(item.Key, out node))
                {
                    _list.Remove(node);

                    _dictionary_key_to_index.Remove(item.Key);

                    return true;
                }
            }

            return false; ;
        }

        #region Неинтересные переопределения
        public U this[T key] { get => _dictionary_key_to_index[key].Value; set => _dictionary_key_to_index[key].Value = value; }

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
            _dictionary_key_to_index.Select(item => new KeyValuePair<T, U> (item.Key, item.Value.Value)).ToArray().CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<T, U>> GetEnumerator()
        {
            return _dictionary_key_to_index.Select(item => new KeyValuePair<T, U>(item.Key, item.Value.Value)).GetEnumerator();
        }


        public bool TryGetValue(T key, [MaybeNullWhen(false)] out U value)
        {
            CustomLinkedListNode<U> val;
            if (_dictionary_key_to_index.TryGetValue(key, out val))
            {
                value = val.Value;
                return true;
            }

            value = default(U);

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary_key_to_index.Select(item => new KeyValuePair<T, U>(item.Key, item.Value.Value)).GetEnumerator();
        }
        #endregion
    }

    //Увы, в C# нет LinkedList-a со свободной манипуляцией Next/Prev

    /// <summary>
    /// Кастомный LinkedList<T>
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class CustomLinkedList<T> : ICollection<T>
    { 
        private bool initialized = false;

        private int Len = 0;
        /// <summary>
        /// Ссылка на первый элемент
        /// </summary>
        public CustomLinkedListNode<T>? FirstNode;

        /// <summary>
        /// Ссылка на последний элемент
        /// </summary>
        public CustomLinkedListNode<T>? LastNode;

        public int Count => Len;

        public bool IsReadOnly => throw new NotImplementedException();

        /// <summary>
        /// Добавляет элемент в конец списка
        /// </summary>
        /// <param name="value">Значение элемента</param>
        /// <returns>Ссылка на элемент списка</returns>
        public CustomLinkedListNode<T> AddLast(T value)
        {
            var node = new CustomLinkedListNode<T>(value);
            Len++;
            initialize(node);

            node.Previos = LastNode;
            LastNode.Next = node;
            LastNode = node;

            return node;
        }

        /// <summary>
        /// Отчищает список
        /// </summary>
        public void Clear()
        {

        }

        /// <summary>
        /// Удаляет элемент из списка сохраняя порядок элементов и основные ссылки
        /// </summary>
        /// <param name="node">элемент для удаления</param>
        public void Remove(CustomLinkedListNode<T> node)
        {
            //Проверяем ссылку на FirstNode
            if (node == FirstNode)
            {
                FirstNode = node.Next;
            }
            Len--;
            //Проверяем ссылку на LastNode
            if (node == LastNode)
            {
                LastNode = node.Previos;
            }

            if (node.Previos != null)
                node.Previos.Next = node.Next;

            if (node.Next != null)
                node.Next.Previos = node.Previos;
        }

        /// <summary>
        /// Инициализирует список указанным элементом
        /// </summary>
        /// <param name="node">Элемент для инициализации списка</param>
        private void initialize(CustomLinkedListNode<T> node)
        {
            if (!initialized)
            {
                FirstNode = node;
                LastNode = node;
                initialized = true;
            }
        }

        /// <summary>
        /// Возвращает энумератор для получения списка значений словаря в порядке вставки
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new CustomListEnumerator<T>(this);
        }

        /// <summary>
        /// Заглушка
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(T item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            CustomLinkedListNode<T> node = FirstNode;
            int pos = arrayIndex;

            while(node!=null)
            {
                array[pos] = node.Value;
                node = node.Next;
                pos++;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// IEnumerator для CustomLinkedList<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    public class CustomListEnumerator<T> : IEnumerator<T>
    {
        private CustomLinkedList<T> _list;

        public CustomListEnumerator(CustomLinkedList<T> list)
        {
            _list = list;
        }

        private bool initialized = false;

        private CustomLinkedListNode<T>? node;

        public T Current => node.Value;

        object IEnumerator.Current => node.Value;

        public void Dispose()
        {
            //TODO: Add dispose
        }

        public bool MoveNext()
        {
            if (!initialized)
            {
                node = _list.FirstNode;
                initialized = true;

                if (node != null)
                    return true;
            }

            node = node.Next;

            return node != null;
        }

        public void Reset()
        {
            node = null;
            initialized = false;
        }
    }

    /// <summary>
    /// Кастомные элементы LinkedList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CustomLinkedListNode<T>
    {
        public CustomLinkedListNode(T value)
        {
            Value = value;
        }
        /// <summary>
        /// Ссылка на следующий элемент
        /// </summary>
        public CustomLinkedListNode<T>? Next;
        /// <summary>
        /// Ссылка на предыдущий элемент
        /// </summary>
        public CustomLinkedListNode<T>? Previos;

        /// <summary>
        /// Значение элемента
        /// </summary>
        public T? Value;
    }

}
