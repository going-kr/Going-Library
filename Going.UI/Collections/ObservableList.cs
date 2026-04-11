using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Collections
{
    /// <summary>
    /// 변경 감지를 지원하는 관찰 가능한 리스트입니다.
    /// </summary>
    /// <typeparam name="T">리스트에 저장할 요소의 타입</typeparam>
    public class ObservableList<T> : IList<T>
    {
        [JsonInclude]
        private readonly List<T> ls = new List<T>();

        /// <summary>
        /// 리스트의 내용이 변경되었는지 여부를 가져오거나 설정합니다.
        /// </summary>
        public bool Changed { get; set; }
        /// <summary>
        /// 리스트에 포함된 요소의 수를 가져옵니다.
        /// </summary>
        public int Count => ls.Count;
        /// <summary>
        /// 리스트가 읽기 전용인지 여부를 가져옵니다. 항상 false를 반환합니다.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// 지정된 인덱스에 있는 요소를 가져오거나 설정합니다.
        /// </summary>
        /// <param name="index">요소의 인덱스</param>
        /// <returns>지정된 인덱스의 요소</returns>
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

        /// <summary>
        /// 리스트의 끝에 항목을 추가합니다.
        /// </summary>
        /// <param name="item">추가할 항목</param>
        public void Add(T item) { ls.Add(item); Changed = true; }
        /// <summary>
        /// 리스트의 끝에 여러 항목을 추가합니다.
        /// </summary>
        /// <param name="items">추가할 항목 컬렉션</param>
        public void AddRange(IEnumerable<T> items) { ls.AddRange(items); Changed = true; }
        /// <summary>
        /// 지정된 인덱스에 항목을 삽입합니다.
        /// </summary>
        /// <param name="index">삽입할 위치의 인덱스</param>
        /// <param name="item">삽입할 항목</param>
        public void Insert(int index, T item) { ls.Insert(index, item); Changed = true; }
        /// <summary>
        /// 리스트에서 특정 항목을 제거합니다.
        /// </summary>
        /// <param name="item">제거할 항목</param>
        /// <returns>항목이 성공적으로 제거되면 true, 그렇지 않으면 false</returns>
        public bool Remove(T item) { var b = ls.Remove(item); if (b) Changed = true; return b; }
        /// <summary>
        /// 지정된 인덱스의 항목을 제거합니다.
        /// </summary>
        /// <param name="index">제거할 항목의 인덱스</param>
        public void RemoveAt(int index) { ls.RemoveAt(index); Changed = true; }

        /// <summary>
        /// 리스트의 모든 항목을 제거합니다.
        /// </summary>
        public void Clear() { ls.Clear(); Changed = true; }
        /// <summary>
        /// 리스트에 특정 항목이 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="item">확인할 항목</param>
        /// <returns>항목이 포함되어 있으면 true, 그렇지 않으면 false</returns>
        public bool Contains(T item) => ls.Contains(item);
        /// <summary>
        /// 리스트의 요소를 배열에 복사합니다.
        /// </summary>
        /// <param name="array">대상 배열</param>
        /// <param name="arrayIndex">복사를 시작할 배열의 인덱스</param>
        public void CopyTo(T[] array, int arrayIndex) => ls.CopyTo(array, arrayIndex);
        /// <summary>
        /// 특정 항목의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="item">검색할 항목</param>
        /// <returns>항목의 인덱스, 없으면 -1</returns>
        public int IndexOf(T item) => ls.IndexOf(item);

        /// <summary>
        /// 리스트의 열거자를 반환합니다.
        /// </summary>
        /// <returns>리스트의 열거자</returns>
        public IEnumerator<T> GetEnumerator() => ls.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ls.GetEnumerator();
    }
}
