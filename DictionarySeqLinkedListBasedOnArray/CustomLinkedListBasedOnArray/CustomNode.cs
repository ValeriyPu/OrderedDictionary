namespace IDictionarySeq.DictionarySeqLinkedListBasedOnArray.CustomLinkedListBasedOnArray
{
    /// <summary>
    /// Кастомные элементы LinkedList
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public struct CustomNode<T>
    {
        public CustomNode(T value)
        {
            Value = value;
            IsActive = true;
        }

        /// <summary>
        /// Ссылка на следующий элемент
        /// </summary>
        public int? Next;

        /// <summary>
        /// Ссылка на предыдущий элемент
        /// </summary>
        public int? Previos;

        /// <summary>
        /// Значение элемента
        /// </summary>
        public T? Value;

        /// <summary>
        /// Содержит ли значение или нет
        /// </summary>
        public bool IsActive;

        /// <summary>
        /// Помечает элемент как отсутствующий в списке
        /// </summary>
        public void Delete()
        {
            IsActive = false;
            Next = -1;
            Previos = -1;
            Value = default(T); 
        }
    }
}
