using Going.UI.Controls;
using Going.UI.Datas;
using Going.UI.Themes;
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
        // Percent, Auto, Pixel 일일이 귀찮아서 나중에 string을 Parsing해서 사용하게 만들었습니다.
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
        protected override void OnDraw(SKCanvas canvas)
        {
            if(Design != null && Design.DesignMode)
            {
                var rt = Areas()["Content"];
                var rts = Util.Grid(rt, Columns.ToArray(), Rows.ToArray());

                using var pe = SKPathEffect.CreateDash([1, 2], 2);
                using var p = new SKPaint { };

                p.IsStroke = true;
                p.StrokeWidth = 1;
                p.Color = GoTheme.Current.Base3;
                p.PathEffect = pe;
                foreach(var v in rts)
                {
                    var vrt = v;
                    vrt.Inflate(-0.5F, -0.5F);
                    canvas.DrawRect(vrt, p);
                }
                p.PathEffect = null;
            }

            base.OnDraw(canvas);
        }

        protected override void OnLayout()
        {
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
                        c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //if (c.Fill) c.Bounds = Util.Int(Util.FromRect(vrt, c.Margin));
                        //else c.Bounds = Util.Int(Util.FromRect(vrt.Left, vrt.Top, Math.Min(vrt.Width, c.Bounds.Width), Math.Min(vrt.Height, c.Bounds.Height)));
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
