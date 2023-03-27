using System.Collections;

namespace IDictionarySeq.CustomLinkedListBasedOnArray
{

    //Увы, в C# нет LinkedList-a со свободной манипуляцией Next/Prev

    /// <summary>
    /// Кастомный LinkedList<T> на основе массива со структурами
    /// </summary>
    /// <typeparam name="T">Тип данных</typeparam>
    public class CustomLinkedListBasedOnArray<T> : ICollection<T>
    {
        private bool initialized = false;

        /// <summary>
        /// Массив с собственно связанным списком
        /// </summary>
        private CustomNode<T>[] values = new CustomNode<T>[1];

        /// <summary>
        /// Длина списка
        /// </summary>
        private int Len = 0;

        /// <summary>
        /// Ссылка на первый элемент
        /// </summary>
        public int? FirstNode;

        /// <summary>
        /// Ссылка на последний элемент
        /// </int>
        public int? LastNode;

        public int Count => Len;

        public int Capacity => values.Length;

        public bool IsReadOnly => false;

        /// <summary>
        /// Добавляет элемент в конец списка
        /// </summary>
        /// <param name="value">Значение элемента</param>
        /// <returns>Ссылка на элемент списка</returns>
        public int AddLast(T value)
        {
            var node = new CustomNode<T>(value);

            //Массив уже заполнен
            if (Len + 1 >= values.Length)
                ReinitializeArray();

            var index = GetFreeIndex();
            if (!initialize(node))
            {
                node.Previos = LastNode;
                values[(int)LastNode].Next = index;
            }

            values[index] = node;

            LastNode = index;

            Len++;
            return index;
        }

        /// <summary>
        /// Увеличивает размер массива для LinkedList в 2 раза
        /// </summary>
        public void ReinitializeArray()
        {
            var array2 = new CustomNode<T>[values.Length * 2];
            for (int pos = 0; pos < values.Length; pos++)
            {
                array2[pos] = values[pos];
            }

            values = array2;
        }

        /// <summary>
        /// Получает первый попавшийся индекс со свободным местом
        /// </summary>
        /// <returns>Индекс в массиве доступный для вставки нового элемента</returns>
        private int GetFreeIndex()
        {
            for (int pos = 0; pos < values.Length; pos++)
            {
                if (!values[pos].IsActive)
                    return pos;
            }

            return -1;
        }

        /// <summary>
        /// Отчищает список
        /// </summary>
        public void Clear()
        {
            Len = 0;
            FirstNode = null;
            LastNode = null;
        }

        /// <summary>
        /// Удаляет элемент из списка сохраняя порядок элементов и основные ссылки
        /// </summary>
        /// <param name="node">элемент для удаления</param>
        public void Remove(CustomNode<T> node)
        {
            var pos = GetNodePosition(node).Value;

            //Проверяем ссылку на FirstNode
            if (pos == FirstNode)
            {
                FirstNode = node.Next;
            }

            //Проверяем ссылку на LastNode
            if (pos == LastNode)
            {
                LastNode = node.Previos;
            }

            if (node.Previos == null & node.Next == null)
            {
                initialized = false;
                FirstNode = null;
                LastNode = null;
                Len = 0;
            }

            if (node.Previos != null)
                values[(int)node.Previos].Next = node.Next;

            if (node.Next != null)
                values[(int)node.Next].Previos = node.Previos;

            values[pos].Delete();

            Len--;
        }

        /// <summary>
        /// Инициализирует список указанным элементом
        /// </summary>
        /// <param name="node">Элемент для инициализации списка</param>
        private bool initialize(CustomNode<T> node)
        {
            if (!initialized)
            {
                values = new CustomNode<T>[1];

                FirstNode = 0;
                LastNode = 0;
                initialized = true;
                Len = 0;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Возвращает энумератор для получения списка значений словаря в порядке вставки
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new CustomLinkedListBasedOnArrayEnumerator<T>(this);
        }

        public IEnumerator<CustomNode<T>> GetRawEnumerator()
        {
            return new CustomLinkedListBasedOnArrayRawEnumerator<T>(this);
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
            AddLast(item);
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            int? index = (int)FirstNode;

            int pos = arrayIndex;

            while (index != null)
            {
                array[pos] = values[index.Value].Value;

                index = values[index.Value].Next;

                pos++;
            }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Прячет внутреннюю коллекцию элементов от доступа извне
        /// </summary>
        /// <param name="key">Индекс элемента</param>
        /// <returns>Элемент</returns>
        public CustomNode<T> this[int key]
        {
            get
            {
                return values[key];
            }
            set
            {
                var oldNode = values[key];

                oldNode.Value = value.Value;

                values[key] = oldNode;
            }
        }

        /// <summary>
        /// Возвращает позицию ноды в двусвязном списке за О(1)
        /// Позволяет сэкономить еще один Int ) 
        /// </summary>
        /// <param name="node">Структура, индекс которой нужно найти</param>
        /// <returns>Индекс структуры в массиве</returns>
        public int? GetNodePosition(CustomNode<T> node)
        {
            if (node.Previos != null)
                return values[(int)node.Previos].Next;
            if (node.Next != null)
                return values[(int)node.Next].Previos;

            return 0;
        }
    }
}
