using Going.UI.Containers;
using Going.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Collections
{
    public class GoControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        public IGoControl this[int index]
        {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public int Count => list.Count;
        #endregion

        #region Member Variable
        private IGoContainer? Container;

        [JsonInclude]
        internal List<IGoControl> list = [];
        #endregion

        #region Event
        public event EventHandler? Changed;
        #endregion

        #region Constructor
        public GoControlCollection() { }
        public GoControlCollection(IEnumerable<IGoControl> list) { this.list = new List<IGoControl>(list); }
        #endregion

        #region Method
        #region Add
        public void Add(IGoControl control)
        {
            if (control == null) throw new Exception("컨트롤이 null 일 수 없습니다.");
            if (Container != null) control.Parent = Container;

            list.Add(control);
            InvokeChanged();
        }
        #endregion
        #region AddRange
        public void AddRange(IEnumerable<IGoControl> controls)
        {
            foreach (var control in controls)
            {
                if (control == null) throw new Exception("컨트롤이 null 일 수 없습니다.");
                if (Container != null) control.Parent = Container;
            }

            list.AddRange(controls);
            InvokeChanged();
        }
        #endregion
        #region Remove
        public void Remove(IGoControl value)
        {
            if (value != null && Contains(value))
            {
                list.Remove(value);
                InvokeChanged();
            }
        }
        #endregion
        #region Contains
        public bool Contains(IGoControl value) => list.Contains(value);
        #endregion
        #region Clear
        public void Clear() => list.Clear();
        #endregion

        #region Initialize
        public void Initialize(IGoContainer container)
        {
            this.Container = container;
            foreach (var c in this) c.Parent = container;
        }
        #endregion

        #region InvokeChanged
        protected void InvokeChanged() => Changed?.Invoke(this, EventArgs.Empty);
        #endregion

        #region Implements
        public IEnumerator<IGoControl> GetEnumerator() => list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => list.GetEnumerator();
        #endregion
        #endregion

    }
}
