using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoTableLayoutPanel : GoContainer
    {
        #region Properties
        public List<string> Columns { get; set; } = [];
        public List<string> Rows { get; set; } = [];

        [JsonInclude]
        public override GoTableLayoutControlCollection Childrens { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoTableLayoutPanel(GoTableLayoutControlCollection childrens) => Childrens = childrens;
        public GoTableLayoutPanel() { }
        #endregion

        #region Override
        protected override void OnLayout()
        {
            var b = Bounds;
            var rt = Areas()["Content"];
            var rts = Util.Grid(rt, Columns.ToArray(), Rows.ToArray());
            foreach (var c in Childrens)
            {
                try
                {
                    var idx = Childrens.Indexes[c.Id];
                    if (idx != null)
                    {
                        var vrt = Util.Merge(rts, idx.Column, idx.Row, idx.ColSpan, idx.RowSpan);
                        if (c.Fill) c.Bounds = Util.FromRect(vrt, c.Margin);
                        else c.Bounds = Util.FromRect(vrt.Left, vrt.Top, c.Bounds.Width, c.Bounds.Height);
                    }
                }
                catch { }
            }
        }
        #endregion

    }

    #region class : GoTableIndex
    public class GoTableIndex
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public int RowSpan { get; set; } = 1;
        public int ColSpan { get; set; } = 1;
    }
    #endregion

    public class GoTableLayoutControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        public Dictionary<Guid, GoTableIndex> Indexes { get; } = [];
        public List<IGoControl> Controls { get; } = [];
        #endregion
  
        #region Indexer
        public IGoControl? this[int index] => Controls[index];
        public IGoControl? this[int col, int row] => Controls.FirstOrDefault(c => Indexes.TryGetValue(c.Id, out var index) && index.Column == col && index.Row == row);
        public GoTableIndex? this[IGoControl? control] => control != null && Indexes.TryGetValue(control.Id, out var index) ? index : null;
        #endregion

        #region Method
        public void Add(IGoControl control, int col, int row, int colspan = 1, int rowspan = 1)
        {
            Controls.Add(control);
            Indexes.Add(control.Id, new GoTableIndex { Column = col, Row = row, ColSpan = colspan, RowSpan = rowspan });
        }

        public bool Remove(IGoControl control)
        {
            if (Controls.Remove(control))
            {
                Indexes.Remove(control.Id);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            Controls.Clear();
            Indexes.Clear();
        }

        public bool Contains(IGoControl control) => control != null && Controls.Contains(control);

        public IEnumerator<IGoControl> GetEnumerator() => Controls.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Controls.GetEnumerator();
        #endregion
    }
}
