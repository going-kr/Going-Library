using Going.UI.Controls;
using Going.UI.Utils;
using SkiaSharp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Going.UI.Containers
{
    public class GoGridLayoutPanel : GoContainer
    {
        #region Properties
        public List<GoGridLayoutPanelRow> Rows { get; set; } = [];

        [JsonInclude]
        public override GoGridLayoutControlCollection Childrens { get; } = [];
        #endregion

        #region Constructor
        [JsonConstructor]
        public GoGridLayoutPanel(GoGridLayoutControlCollection childrens) => Childrens = childrens;
        public GoGridLayoutPanel() { }
        #endregion

        #region Override
        protected override void OnLayout()
        {
            var b = Bounds;
            var rt = Areas()["Content"];
            #region make rts
            var rows = Util.Rows(rt, Rows.Select(x => x.Height).ToArray());
            var rts = new Dictionary<int, Dictionary<int, SKRect>>();
            for (int i = 0; i < rows.Length; i++)
            {
                var vrt = rows[i];
                rts.Add(i, []);

                var cols = Util.Columns(vrt, Rows[i].Columns.ToArray());
                for (int j = 0; j < cols.Length; j++) rts[i].Add(j, cols[j]);
            }
            #endregion

            foreach (var c in Childrens)
            {
                try
                {
                    var idx = Childrens.Indexes[c.Id];
                    if (idx != null && rts.TryGetValue(idx.Row, out Dictionary<int, SKRect>? vrow) && vrow.TryGetValue(idx.Column, out SKRect vrt))
                    {
                        if (c.Fill) c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        else c.Bounds = Util.Int(Util.FromRect(vrt.Left, vrt.Top, c.Bounds.Width, c.Bounds.Height));
                    }
                }
                catch { }
            }
        }
        #endregion

        #region Method
        public void AddRow(string height, string[] columns)
        {
            Rows.Add(new GoGridLayoutPanelRow { Height = height, Columns = [.. columns] });
        }
        #endregion
    }

    #region class
    public class GoGridLayoutPanelRow
    {
        public string Height { get; set; } = "100%";
        public List<string> Columns { get; set; } = [];
    }
    #endregion
    #region class : GoGridIndex
    public class GoGridIndex
    {
        public int Row { get; set; }
        public int Column { get; set; }
    }
    #endregion

    public class GoGridLayoutControlCollection : IEnumerable<IGoControl>
    {
        #region Properties
        [JsonInclude] public Dictionary<Guid, GoGridIndex> Indexes { get; } = [];
        [JsonInclude] public List<IGoControl> Controls { get; } = [];
        #endregion

        #region Indexer
        public IGoControl? this[int index] => Controls[index];
        public IGoControl? this[int col, int row] => Controls.FirstOrDefault(c => Indexes.TryGetValue(c.Id, out var index) && index.Column == col && index.Row == row);
        public GoGridIndex? this[IGoControl? control] => control != null && Indexes.TryGetValue(control.Id, out var index) ? index : null;
        #endregion

        #region Method
        public void Add(IGoControl control, int col, int row)
        {
            Controls.Add(control);
            Indexes.Add(control.Id, new GoGridIndex { Column = col, Row = row });
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
