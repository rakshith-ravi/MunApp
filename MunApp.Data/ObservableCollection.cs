using System;
using System.Collections;
using System.Collections.Generic;

namespace MunApp.Common
{
    public class ObservableList<T> : IList<T>
    {
        private List<T> list;

        public event EventHandler<ItemEventArgs<T>> ItemAdded;
        public event EventHandler<ItemEventArgs<T>> ItemRemoved;

        public ObservableList()
        {
            list = new List<T>();
        }
        public ObservableList(int capacity)
        {
            list = new List<T>(capacity);
        }
        public ObservableList(IEnumerable<T> collection)
        {
            list = new List<T>(collection);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return list.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public void Add(T item)
        {
            list.Add(item);
            if (ItemAdded != null)
                ItemAdded(this, new ItemEventArgs<T>(list.Count - 1));
        }

        public void AddRange(IEnumerable<T> collection)
        {
            list.AddRange(collection);
            if (ItemAdded != null)
                ItemAdded(this, new ItemEventArgs<T>(list.Count - 1));
        }

        public void Clear()
        {
            list.Clear();
            if (ItemRemoved != null)
                ItemRemoved(this, new ItemEventArgs<T>(0));
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
            if (ItemAdded != null)
                ItemAdded(this, new ItemEventArgs<T>(index));
        }

        public bool Remove(T item)
        {
            if (ItemRemoved != null)
                ItemRemoved(this, new ItemEventArgs<T>(list.IndexOf(item)));
            return list.Remove(item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
            if (ItemRemoved != null)
                ItemRemoved(this, new ItemEventArgs<T>(index));
        }

        public T[] ToArray()
        {
            return list.ToArray();
        }

        public void Sort()
        {
            list.Sort();
        }

        public void Sort(Comparison<T> comparison)
        {
            list.Sort(comparison);
        }

        public void Sort(IComparer<T> comparer)
        {
            list.Sort(comparer);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)list).GetEnumerator();
        }
    }

    public class ItemEventArgs<T> : EventArgs
    {
        public int Index { get; set; }

        public ItemEventArgs(int index)
        {
            Index = index;
        }
    }
}
