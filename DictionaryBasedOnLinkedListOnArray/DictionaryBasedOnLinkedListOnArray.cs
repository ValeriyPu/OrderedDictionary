using IDictionarySeq.CustomLinkedListBasedOnArray;
using System.Net.WebSockets;

namespace IDictionarySeq
{
    //Более простой и эффективный вариант

    /// <summary>
    /// Кастомный словарь с сохранением порядка вставки элементов при перечислении значений]
    /// Более трудоемкий вариант, приходится переопределять практически все методы интерфейса
    /// </summary>
    /// <typeparam name="T">Тип ключа</typeparam>
    /// <typeparam name="U">Тип значения</typeparam>
    internal class DictionaryBasedOnLinkedListOnArray<T, U>
    {
        public DictionaryBasedOnLinkedListOnArray()
        {
            _list = new CustomLinkedListBasedOnArray<KeyValuePair<T, U>>();
            _keysList = new int[_list.Capacity];
            initKeyList(_keysList,0, -1);
        }

        private void initKeyList(int[] keysList, int startElem, int v)
        {
            for(int pos= startElem; pos<keysList.Length;pos++)
            {
                keysList[pos] = v;
            }
        }

        /// <summary>
        /// Связанный список для хранения элементов в порядке вставки
        /// </summary>
        private CustomLinkedListBasedOnArray<KeyValuePair<T, U>> _list;

        /// <summary>
        /// Список соответствия Ключ - Индекс в списке
        /// </summary>
        private int[] _keysList;


        private int GetElementIndex(T key)
        {
            var hashcode = key.GetHashCode();
            var keyhash = hashcode  % _list.Capacity;

            return keyhash;
        }

        private void AddElement(T key, U value)
        {
            var index= GetElementIndex(key);
            if ((_keysList[index]!=-1)||(_list.Capacity<=_list.Count+1))
            {
                IncreaseArraysSize();
                AddElement(key, value);
            }
            else
            {
                var pos = _list.AddLast(new KeyValuePair<T, U>(key, value));

                _keysList[index] = pos;
            }
        }

        private void RemoveElement(T key)
        {
            var index = GetElementIndex(key);
   
            _list.Remove(_list[_keysList[index]]);
            _keysList[index] = -1;
        }

        private U GetElement(T key)
        {
            var index = GetElementIndex(key);
            var pos = _keysList[index];
            var elem = _list[pos];

            return elem.Value.Value;
        }

        private void IncreaseArraysSize()
        {
            _list.ReinitializeArray();

            var resKeys = new int[_keysList.Length * 2];

            initKeyList(resKeys, 0, -1);


            if (_list.Count != 0)
            {
                var enumerator = _list.GetRawEnumerator();

                for (int pos = 0; pos < _list.Count; pos++)
                {
                    enumerator.MoveNext();
                    var item = enumerator.Current;

                    var hash = GetElementIndex(item.Value.Key);
                    var pos_in_list = _list.GetNodePosition(item);

                    resKeys[hash] = pos_in_list.Value;
                }
            }

            _keysList = resKeys;
        }

        public void Add(T key, U value)
        {
            AddElement(key, value);
        }

        public void Remove(T key)
        {
            RemoveElement(key);
        }

        public IEnumerable<KeyValuePair<T, U>> Values => _list;
    }
}

