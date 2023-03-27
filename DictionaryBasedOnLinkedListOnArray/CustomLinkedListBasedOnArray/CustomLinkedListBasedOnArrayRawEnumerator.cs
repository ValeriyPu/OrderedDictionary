using System.Collections;

namespace IDictionarySeq.CustomLinkedListBasedOnArray
{

    /// <summary>
    /// IEnumerator для CustomLinkedListBasedOnArrayEnumerator<typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">Тип</typeparam>
    public class CustomLinkedListBasedOnArrayRawEnumerator<T> : IEnumerator<CustomNode<T>>
    {
        private CustomLinkedListBasedOnArray<T> _list;

        public CustomLinkedListBasedOnArrayRawEnumerator(CustomLinkedListBasedOnArray<T> list)
        {
            _list = list;
        }

        private bool initialized = false;

        private CustomNode<T>? node;

        public CustomNode<T> Current => node ?? default;

        object IEnumerator.Current => node.Value;

        public void Dispose()
        {
            //TODO: Add dispose
        }

        public bool MoveNext()
        {
            if (!initialized)
            {
                node = _list[(int)_list.FirstNode];
                initialized = true;

                if (node != null)
                    return true;
            }

            if (node.Value.Next==null)
            {
                return false;
            }

            node = _list[(int)node.Value.Next];

            return node.Value.IsActive;
        }

        public void Reset()
        {
            node = null;
            initialized = false;
        }
    }
}
