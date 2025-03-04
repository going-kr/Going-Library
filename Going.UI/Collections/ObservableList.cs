using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Collections
{
    public class ObservableList<T> : IList<T>
    {
        [JsonInclude]
        private readonly List<T> ls = new List<T>();

        public bool Changed { get; set; }
        public int Count => ls.Count;
        public bool IsReadOnly => false;

        public T this[int index]
        {
            get => ls[index];
            set
            {
                if (!ls[index]?.Equals(value) ?? true)
                {
                    ls[index] = value;
                    Changed = true;
                }
            }
        }

        public void Add(T item) { ls.Add(item); Changed = true; }
        public void AddRange(IEnumerable<T> items) { ls.AddRange(items); Changed = true; }
        public void Insert(int index, T item) { ls.Insert(index, item); Changed = true; }
        public bool Remove(T item) { var b = ls.Remove(item); if (b) Changed = true; return b; }
        public void RemoveAt(int index) { ls.RemoveAt(index); Changed = true; }

        public void Clear() { ls.Clear(); Changed = true; }
        public bool Contains(T item) => ls.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => ls.CopyTo(array, arrayIndex);
        public int IndexOf(T item) => ls.IndexOf(item);
        
        public IEnumerator<T> GetEnumerator() => ls.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ls.GetEnumerator();
    }
}
